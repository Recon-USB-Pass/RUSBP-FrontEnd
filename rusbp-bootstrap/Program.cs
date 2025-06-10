// Program.cs actualizado con logs para rastreo detallado en Registrar ROOT

using rusbp_bootstrap.Api;
using rusbp_bootstrap.Core;
using rusbp_bootstrap.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ApiUsbRole = rusbp_bootstrap.Api.UsbRole;

class Program
{
    static string? ultimoRpGenerado = null;

    public static void SetUltimoRpGenerado(string rp)
    {
        ultimoRpGenerado = rp;
    }

    static string LeerPasswordOculta(string prompt)
    {
        Console.Write(prompt);
        var sb = new StringBuilder();
        while (true)
        {
            var k = Console.ReadKey(true);
            if (k.Key == ConsoleKey.Enter) break;
            if (k.Key == ConsoleKey.Backspace && sb.Length > 0)
            { sb.Length--; Console.Write("\b \b"); }
            else if (!char.IsControl(k.KeyChar))
            { sb.Append(k.KeyChar); Console.Write('*'); }
        }
        Console.WriteLine();
        return sb.ToString();
    }

    static void EsperarMontaje(string letter)
    {
        for (int i = 0; i < 12 && !Directory.Exists($"{letter}:/"); i++)
            Task.Delay(500).Wait();
    }

    static string Ejecutar(string file, string args)
    {
        Console.WriteLine($"> Ejecutando: {file} {args}");
        var p = Process.Start(new ProcessStartInfo
        {
            FileName = file,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        })!;
        string all = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
        p.WaitForExit();
        Console.WriteLine($"<< Salida: \n{all}\n");
        return all;
    }

    static string ObtenerRpRoot(string drive)
    {
        //Console.WriteLine("[DEBUG] Obteniendo RP desde manage-bde");
        string Grab(string t) => Regex.Match(t, @"\b\d{6}(?:-\d{6}){7}\b").Value;

        var rp = Grab(Ejecutar("manage-bde.exe", $"-protectors {drive}: -get"));
        if (!string.IsNullOrEmpty(rp)) return rp;

        //Console.WriteLine("[DEBUG] No se encontró RP, creando uno nuevo");
        Ejecutar("powershell.exe",
            $"-Command \"Add-BitLockerKeyProtector -MountPoint '{drive}:' -RecoveryPasswordProtector | Out-String\"");
        rp = Grab(Ejecutar("manage-bde.exe", $"-protectors {drive}: -get"));
        if (!string.IsNullOrEmpty(rp)) return rp;

        Console.WriteLine("[WARN] No se encontró ni generó RP automáticamente, pidiendo ingreso manual");
        Console.Write("Ingrese los 48 dígitos (sin guiones): ");
        var manual = (Console.ReadLine() ?? "").Trim();
        if (!Regex.IsMatch(manual, @"^\d{48}$")) throw new Exception("RP_root inválida.");
        return Regex.Replace(manual, @"(\d{6})(?=\d)", "$1-");
    }

    static async Task<(bool ok, string msg)> RegistrarRootAsync(
    UsbDeviceInfo usb, string rpRoot, string backendIp)
    {
        try
        {
            //Console.WriteLine("[INFO] Iniciando registro de ROOT");
            //Console.WriteLine($"[INFO] RP usada para cifrar blob: {rpRoot}");

            var sysDir = Path.Combine(usb.DriveLetter + ":/", "rusbp.sys");
            Directory.CreateDirectory(sysDir);

            var rpPath = Path.Combine(sysDir, ".btlk-rp");
            if (File.Exists(rpPath))
            {
                //Console.WriteLine("[DEBUG] Validando RP contra .btlk-rp existente");
                try
                {
                    var cifrado = File.ReadAllBytes(rpPath);
                    var original = CryptoHelper.DecryptToString(cifrado, rpRoot);
                    //Console.WriteLine($"[INFO] RP validada exitosamente: {original}");
                    rpRoot = original;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[ERROR] RP inválida: {ex.Message}");
                    return (false, "La RP ingresada no coincide con la original guardada.");
                }
            }

            var blob = CryptoHelper.EncryptString(rpRoot, rpRoot);

            // Guardar archivos locales
            File.WriteAllBytes(Path.Combine(sysDir, ".btlk"), blob);
            File.WriteAllBytes(Path.Combine(sysDir, ".btlk-ip"), CryptoHelper.EncryptString(backendIp, rpRoot));
            File.WriteAllBytes(Path.Combine(sysDir, ".btlk-rp"), blob);

            // Validar que el blob sea válido
            try
            {
                //Console.WriteLine("[DEBUG] Probando descifrado de prueba interno");
                var prueba = CryptoHelper.DecryptToString(blob, rpRoot);
                //Console.WriteLine("[DEBUG] Descifrado OK");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[ERROR] Descifrado fallido: {ex.Message}");
                return (false, "La clave RP actual no puede descifrar el blob cifrado.");
            }

            // Backend
            //Console.WriteLine("[INFO] Preparando conexión a backend");
            var h = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
            using var http = new HttpClient(h) { BaseAddress = new Uri($"https://{backendIp}:8443") };
            var backend = new BackendClient(http);

            //Console.WriteLine("[INFO] Llamando a backend.RegistrarUsbAsync");
            if (!await backend.RegistrarUsbAsync(usb.Serial, blob[16..], blob[..16], ApiUsbRole.Root))
                return (false, "RegistrarUsb falló.");

            // PKI
            var pkiDir = Path.Combine(usb.DriveLetter + ":/", "pki");
            Directory.CreateDirectory(pkiDir);
            var (certPath, keyPath) = PkiService.GeneratePkcs8KeyPair(usb.Serial, pkiDir);
            var thumb = new X509Certificate2(certPath).Thumbprint!.ToUpperInvariant();

            // Crear usuario
            var admin = new Usuario
            {
                Rut = BootstrapHelpers.Prompt("RUT         :", "Jefe TI", "11.111.111-1"),
                Nombre = BootstrapHelpers.Prompt("Nombre      :", "Empresa", "Admin"),
                Depto = BootstrapHelpers.Prompt("Depto       :", "TI / Seguridad", "TI"),
                Email = BootstrapHelpers.Prompt("Email       :", "email", "admin@empresa.cl"),
                Rol = "Admin",
                Pin = BootstrapHelpers.Prompt("PIN         :", "4−6 dígitos", "1234")
            };

            var created = await backend.CrearUsuarioAsync(new UsuarioDto
            {
                Rut = admin.Rut,
                Nombre = admin.Nombre,
                Depto = admin.Depto,
                Email = admin.Email,
                Rol = admin.Rol,
                Pin = admin.Pin
            });

            if (created is null || !await backend.AsignarUsbAUsuarioAsync(usb.Serial, admin.Rut))
                return (false, "Crear/Asignar usuario falló.");

            // Autotest
            var challenge = await backend.ObtenerChallengeAsync(usb.Serial, File.ReadAllText(certPath));
            if (challenge is null) return (false, "verify-usb falló.");

            var sign = CryptoHelper.FirmarChallenge(File.ReadAllText(keyPath), challenge);
            if (!await backend.ProbarRecoverAsync(usb.Serial, sign, admin.Pin, 0))
                return (false, "Autotest login falló.");

            // Config
            var cfg = new ConfigJson
            {
                Nombre = admin.Nombre,
                Rut = admin.Rut,
                Email = admin.Email,
                Rol = admin.Rol,
                Serial = usb.Serial,
                Fecha = DateTime.UtcNow
            };

            File.WriteAllText(Path.Combine(usb.DriveLetter + ":/", "config.json"),
                JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));

            return (true, "✔️  ROOT registrado correctamente.");
        }
        catch (Exception ex)
        {
            //Console.WriteLine("[FATAL] " + ex);
            return (false, "✖  " + ex.Message);
        }
    }


    /*──────── IP backend ─────*/
    static string PreguntarIp()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("IP/FQDN backend: "); Console.ResetColor();
            var input = (Console.ReadLine() ?? "").Trim();
            if (System.Net.IPAddress.TryParse(input, out var ip)) input = ip.ToString();

            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                if (ping.Send(input, 1200).Status ==
                    System.Net.NetworkInformation.IPStatus.Success)
                    return input;
            }
            catch { }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✖  Sin respuesta de “{input}”.");
            Console.ResetColor();
            Console.Write("¿Guardar igual? (S/N): ");
            if ((Console.ReadLine() ?? "").Trim().ToUpperInvariant() == "S")
                return input;
        }
    }


    /*──────── MAIN ───────*/
    static async Task Main()
    {
        UsbDeviceInfo? usb = null;
        string backendIp = PreguntarIp();

        while (true)
        {
            if (usb is null) { Console.Clear(); usb = UsbManager.SelectUsbDevice(); Console.Clear(); }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[IP]:  {backendIp}");
            Console.WriteLine($"USB :  {usb.DriveLetter ?? "(sin letra)"} | Serial={usb.Serial}");
            Console.ResetColor();

            Console.WriteLine("""
                1) Cambiar IP
                2) Cambiar USB
                3) Limpiar
                4) Descifrar
                5) Cifrar
                6) Registrar ROOT
                7) Ver último Recovery Password generado
                8) Salir
                """);
            Console.Write("\nOpción: ");
            var opt = Console.ReadLine()?.Trim();
            if (opt == "8") break;
            if (opt == "1") { backendIp = PreguntarIp(); continue; }
            if (opt == "2") { usb = null; continue; }

            bool ok = false; string msg = "";

            /*─ 3) Limpiar ─*/
            if (opt == "3")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("⚠️  BORRARÁ todo. ¿Continuar? (S/N): ");
                Console.ResetColor();
                if (!Console.ReadLine()!.Trim().Equals("S", StringComparison.OrdinalIgnoreCase)) { Console.Clear(); continue; }

                bool unlocked = false;
                if (BitLockerManager.IsDriveLocked(usb.DriveLetter))
                {
                    var p = LeerPasswordOculta("Clave BitLocker: ");
                    if (!BitLockerManager.UnlockDrive(usb.DriveLetter, p))
                    { msg = "✖  No se pudo desbloquear."; goto Show; }
                    unlocked = true; EsperarMontaje(usb.DriveLetter);
                }

                try { BitLockerManager.LimpiarUnidadBestEffort(usb.DriveLetter); ok = true; msg = "✔️  Limpieza completada."; }
                catch (Exception ex) { msg = "✖  " + ex.Message; }

            }

            /*─ 4) Descifrar ─*/
            else if (opt == "4")
            {
                var p = LeerPasswordOculta("Clave BitLocker: ");
                if (BitLockerManager.UnlockDrive(usb.DriveLetter, p) && BitLockerManager.DecryptDrive(usb.DriveLetter))
                { ok = true; msg = "Unidad descifrada."; }
                else msg = "✖  Error descifrando.";
            }

            /*─ 5) Cifrar ─*/
            else if (opt == "5")
            {
                var p = LeerPasswordOculta("Nueva clave BitLocker (≥8): ");
                if (p.Length < 8) msg = "Clave ≥8 caracteres.";
                else if (!BitLockerManager.EncryptDrive(usb.DriveLetter, p))
                    msg = "✖  Enable-BitLocker falló.";
                else
                {
                    BitLockerManager.ShowBitLockerProgress(usb.DriveLetter);
                    Directory.CreateDirectory($@"{usb.DriveLetter}:\rusbp.sys");
                    File.WriteAllBytes($@"{usb.DriveLetter}:\rusbp.sys\.btlk",
                        CryptoHelper.EncryptString(p, p));
                    ok = true; msg = "✔️  Unidad cifrada + .btlk guardado.";
                }
            }

            /*──── 6) Registrar ROOT ────*/
            else if (opt == "6")
            {
                var passUnidad = LeerPasswordOculta("Contraseña BitLocker de la unidad: ");
                if (!BitLockerManager.UnlockDrive(usb.DriveLetter, passUnidad))
                { msg = "✖  No se pudo desbloquear."; goto Show; }

                EsperarMontaje(usb.DriveLetter);

                string rpRoot;
                try { rpRoot = ObtenerRpRoot(usb.DriveLetter); }
                catch (Exception ex) { msg = "✖  " + ex.Message; goto Show; }

                (ok, msg) = await RegistrarRootAsync(usb, rpRoot, backendIp);
            }
            if (opt == "7")
            {
                if (string.IsNullOrEmpty(ultimoRpGenerado))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("ℹ️  No hay RP generado en esta sesión.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Último Recovery Password (solo sesión actual):\n{ultimoRpGenerado}");
                }
                Console.ResetColor();
                Console.WriteLine("\nEnter para continuar…");
                Console.ReadLine();
                Console.Clear();
                continue;
            }


        Show:
            Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("\n" + msg);
            Console.ResetColor();
            Console.WriteLine("\nEnter para continuar…");
            Console.ReadLine();
            Console.Clear();
        }
    }
}
