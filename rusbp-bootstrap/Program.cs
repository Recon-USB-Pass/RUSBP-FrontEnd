using rusbp_bootstrap.Core;
using rusbp_bootstrap.Api;
using rusbp_bootstrap.Models;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            // 1. Esperar unidad USB y selección segura
            UsbDeviceInfo? selectedUsb = null;
            while (selectedUsb == null)
                selectedUsb = UsbManager.SelectUsbDevice();

            Console.WriteLine($"Unidad seleccionada: {selectedUsb.DriveLetter ?? "(sin letra)"} - Serial: {selectedUsb.DeviceId}");

            // Proteger contra retiro accidental antes de terminar el proceso
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n⚠️  No retire la unidad seleccionada hasta que el proceso finalice.");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nUnidad seleccionada: {selectedUsb.DriveLetter}: ({selectedUsb.VolumeLabel}) - {selectedUsb.SizeBytes / 1024 / 1024 / 1024} GB");
            Console.ResetColor();

            // 2. Menú principal en bucle seguro
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nSeleccione una opción:");
            Console.WriteLine("1. Limpiar la unidad y descifrarla (BORRAR TODO y eliminar BitLocker)");
            Console.WriteLine("2. Crear/preparar USB root seguro");
            Console.WriteLine("3. Salir");
            Console.ResetColor();

            var menuOpt = Console.ReadLine()?.Trim();
            if (menuOpt == "3")
                break;

            if (menuOpt == "1")
            {
                // Limpiar y descifrar
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️  Esta acción borrará TODOS los archivos y eliminará el cifrado BitLocker de la unidad.");
                Console.Write("¿Desea continuar? (S/N): ");
                Console.ResetColor();

                var resp = Console.ReadLine()?.Trim().ToUpper();
                if (resp != "S")
                {
                    Console.WriteLine("Operación cancelada. Volviendo al menú principal...");
                    continue;
                }

                Console.WriteLine("Ingrese la clave BitLocker de la unidad (deje vacío si no está cifrada):");
                string clave = Console.ReadLine()?.Trim() ?? "";

                // Intentar desbloquear y descifrar si hay clave
                if (!string.IsNullOrWhiteSpace(clave))
                {
                    if (!BitLockerManager.UnlockDrive(selectedUsb.DriveLetter, clave))
                    {
                        Console.WriteLine("✖  No se pudo desbloquear la unidad. Verifique la clave.");
                        continue;
                    }
                    if (!BitLockerManager.DecryptDrive(selectedUsb.DriveLetter))
                    {
                        Console.WriteLine("✖  No se pudo descifrar la unidad con BitLocker.");
                        continue;
                    }
                    Console.WriteLine("✔️  BitLocker eliminado correctamente.");
                }
                else
                {
                    Console.WriteLine("Unidad no cifrada con BitLocker, se procederá a limpiar...");
                }


                // Borrar todo el contenido de la unidad, robusto ante archivos en uso
                bool cleaned = false;
                while (!cleaned)
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(selectedUsb.DriveLetter + @":\");
                        foreach (FileInfo file in di.GetFiles()) file.Delete();
                        foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                        Console.WriteLine("✔️  Unidad limpiada correctamente.");
                        cleaned = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("✖  Error al limpiar la unidad: " + ex.Message);
                        Console.WriteLine("Asegúrese de que ninguna ventana del explorador esté abierta sobre la unidad. Presione ENTER para reintentar...");
                        Console.ReadLine();
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Unidad lista para reutilizar.");
                Console.ResetColor();

                // Permite al usuario volver a ejecutar otra acción sin cerrar
                Console.WriteLine("¿Desea realizar otra operación? (S = Sí, cualquier otra tecla para salir)");
                var again = Console.ReadLine();
                if (again?.Trim().ToUpper() != "S")
                    break;
                continue;
            }
            else if (menuOpt == "2")
            {
                // 3. Crear USB root seguro
                var edition = BootstrapHelpers.GetWindowsEdition();
                bool canBitLocker =
                    edition.IndexOf("pro", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    edition.IndexOf("enterprise", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    edition.IndexOf("education", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    edition.IndexOf("server", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!canBitLocker)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️  ADVERTENCIA: Estás en Windows " + edition + ". Sólo Windows Pro, Enterprise o Education permiten cifrar unidades con BitLocker.");
                    Console.WriteLine("Si continúas sin cifrar, las llaves y datos críticos en el USB NO estarán protegidos en caso de robo o pérdida.");
                    Console.WriteLine("¿Deseas continuar SIN cifrado BitLocker? (S/N):");
                    Console.ResetColor();

                    var resp = Console.ReadLine()?.Trim().ToUpper();
                    if (resp != "S")
                    {
                        Console.WriteLine("Operación cancelada. Volviendo al menú principal...");
                        continue;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("✔️  BitLocker disponible: se recomienda cifrar el USB root para máxima seguridad.");
                    Console.ResetColor();
                }

                string? bitlockerKey = null;
                if (canBitLocker)
                {
                    while (true)
                    {
                        Console.WriteLine("\nClave BitLocker para cifrar el USB root (mínimo 8 caracteres):");
                        bitlockerKey = Console.ReadLine()?.Trim() ?? "";
                        if (!string.IsNullOrWhiteSpace(bitlockerKey) && bitlockerKey.Length >= 8)
                            break;

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("La clave es obligatoria y debe tener al menos 8 caracteres. Intente de nuevo.");
                        Console.ResetColor();
                    }
                }

                // Cifrado y progreso
                if (canBitLocker && bitlockerKey != null)
                {
                    if (!BitLockerManager.EncryptDrive(selectedUsb.DriveLetter, bitlockerKey))
                    {
                        Console.WriteLine("✖ Error al cifrar la unidad con BitLocker.");
                        continue;
                    }

                    BitLockerManager.ShowBitLockerProgress(selectedUsb.DriveLetter);

                    // Guardar la clave cifrada con AES en el propio USB
                    byte[] encryptedKey = CryptoHelper.EncryptString(bitlockerKey, bitlockerKey);
                    string sysDir = Path.Combine(selectedUsb.DriveLetter + @":\", "rusbp.sys");
                    Directory.CreateDirectory(sysDir);
                    File.WriteAllBytes(Path.Combine(sysDir, ".btlk"), encryptedKey);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✔️  USB root cifrado y clave protegida.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️  El USB root será creado SIN cifrado BitLocker. Esto es INSEGURO y NO RECOMENDADO.");
                    Console.ResetColor();
                }

                // Preparar HttpClient y BackendClient
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errs) => true // SOLO DEV, ajusta en prod
                };
                using var http = new HttpClient(handler) { BaseAddress = new Uri("https://192.168.1.209:8443") };
                var backend = new BackendClient(http);

                // Datos usuario root-admin
                var usuario = new Usuario
                {
                    Rut = BootstrapHelpers.Prompt("RUT         :", "idealmente Jefe de TI", "11.111.111-1"),
                    Nombre = BootstrapHelpers.Prompt("Nombre      :", "nombre de la empresa", "admin"),
                    Depto = BootstrapHelpers.Prompt("Departamento:", "root / TI", "TI"),
                    Email = BootstrapHelpers.Prompt("Email       :", "correo corporativo", "admin@empresa.cl"),
                    Rol = "Admin",
                    Pin = BootstrapHelpers.Prompt("PIN         :", "un número inolvidable", "1234")
                };

                // Registrar USB, usuario y asociación
                var okUsb = await backend.RegistrarUsbAsync(selectedUsb.DeviceId);
                if (!okUsb)
                {
                    Console.WriteLine("✖ Error al registrar el USB en backend.");
                    continue;
                }

                var userDto = new rusbp_bootstrap.Api.UsuarioDto
                {
                    Rut = usuario.Rut,
                    Nombre = usuario.Nombre,
                    Depto = usuario.Depto,
                    Email = usuario.Email,
                    Rol = usuario.Rol,
                    Pin = usuario.Pin
                };

                var userCreated = await backend.CrearUsuarioAsync(userDto);
                if (userCreated == null)
                {
                    Console.WriteLine("✖ Error al crear usuario en backend.");
                    continue;
                }

                var okVinc = await backend.AsignarUsbAUsuarioAsync(selectedUsb.DeviceId, usuario.Rut);
                if (!okVinc)
                {
                    Console.WriteLine("✖ Error al asociar USB y usuario en backend.");
                    continue;
                }

                // Generar PKI
                var pkiDir = Path.Combine(selectedUsb.DriveLetter + @":\", "pki");
                Directory.CreateDirectory(pkiDir);
                var (certPath, keyPath) = PkiService.GeneratePkcs8KeyPair(selectedUsb.DeviceId, pkiDir);

                // Crear config.json
                var config = new ConfigJson
                {
                    Nombre = usuario.Nombre,
                    Rut = usuario.Rut,
                    Email = usuario.Email,
                    Rol = "Admin",
                    Serial = selectedUsb.DeviceId,
                    Fecha = DateTime.UtcNow
                };
                string cfgPath = Path.Combine(selectedUsb.DriveLetter + @":\", "config.json");
                File.WriteAllText(cfgPath, System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nUSB-ADM preparado ✔️  ¡Arranca la aplicación!");
                Console.ResetColor();

                // Permite al usuario volver a ejecutar otra acción sin cerrar
                Console.WriteLine("¿Desea realizar otra operación? (S = Sí, cualquier otra tecla para salir)");
                var again = Console.ReadLine();
                if (again?.Trim().ToUpper() != "S")
                    break;
            }
            else
            {
                Console.WriteLine("Opción no válida. Intente de nuevo.");
            }
        }

        Console.WriteLine("Presione cualquier tecla para cerrar...");
        Console.ReadKey();
    }
}






/*

// Program.cs – rusbp-bootstrap (v5 – selección de unidad, menú seguro, BitLocker opcional)
// -----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

// ----------- SELECCIÓN DE UNIDAD USB (antes de cualquier lógica) -----------


void EsperarUnidadUsb()
{
    while (true)
    {
        var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable && d.IsReady).ToArray();
        if (drives.Length > 0) return;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("✖ No se encontró ningún pendrive conectado. Inserte uno y presione cualquier tecla para continuar...");
        Console.ResetColor();
        Console.ReadKey();
    }
}


EsperarUnidadUsb();

DriveInfo[] removableDrives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable && d.IsReady).ToArray();

while (removableDrives.Length == 0)
{
    EsperarUnidadUsb();
    removableDrives = DriveInfo.GetDrives()
        .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
        .ToArray();
}

while (true)
{
    Console.WriteLine("Unidades USB detectadas:");
    for (int i = 0; i < removableDrives.Length; i++)
    {
        var d = removableDrives[i];
        Console.WriteLine($"[{i + 1}] {d.Name} ({d.VolumeLabel}) - {d.TotalSize / 1024 / 1024 / 1024} GB");
    }

    Console.Write("\nSeleccione el número de la unidad con la que desea trabajar: ");
    if (!int.TryParse(Console.ReadLine(), out int sel) || sel < 1 || sel > removableDrives.Length)
    {
        Console.WriteLine("Selección inválida.");
        continue;
    }

    var drive = removableDrives[sel - 1];

    // Confirmar selección
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\nTrabajando sobre la unidad: {drive.Name} ({drive.VolumeLabel}) - {drive.TotalSize / 1024 / 1024 / 1024} GB");
    Console.ResetColor();

    // Verifica que la unidad sigue conectada antes de cada paso clave
    if (!drive.IsReady)
    {
        Console.WriteLine("La unidad seleccionada ha sido retirada. No retire el dispositivo durante el proceso.");
        Console.WriteLine("Presione cualquier tecla para cerrar...");
        Console.ReadKey();
        return;
    }

// Menú principal en bucle
mainMenu:
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nSeleccione una opción antes de continuar:");
        Console.WriteLine("1. Limpiar la unidad y descifrarla (BORRAR TODO y eliminar BitLocker)");
        Console.WriteLine("2. Crear o preparar nuevo USB root seguro (flujo normal)");
        Console.ResetColor();

        string? choice = Console.ReadLine()?.Trim();
        if (choice == "1")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("⚠️  Esta acción borrará TODOS los archivos y eliminará el cifrado BitLocker de la unidad.");
            Console.Write("¿Desea continuar? (S/N): ");
            Console.ResetColor();

            var resp = Console.ReadLine()?.Trim().ToUpper();
            if (resp != "S")
            {
                Console.WriteLine("Operación cancelada. Volviendo al menú principal...");
                continue;
            }

            Console.WriteLine("Ingrese la clave BitLocker de la unidad (deje vacío si no está cifrada):");
            string clave = Console.ReadLine()?.Trim() ?? "";

            // Intentar desbloquear y descifrar si hay clave
            if (!string.IsNullOrWhiteSpace(clave))
            {
                if (!BitLockerHelpers.UnlockDrive(drive.Name.TrimEnd('\\'), clave))
                {
                    Console.WriteLine("✖  No se pudo desbloquear la unidad. Verifique la clave.");
                    Console.WriteLine("Presione cualquier tecla para cerrar...");
                    Console.ReadKey();
                    return;
                }
                if (!BitLockerHelpers.DecryptDrive(drive.Name.TrimEnd('\\')))
                {
                    Console.WriteLine("✖  No se pudo descifrar la unidad con BitLocker.");
                    Console.WriteLine("Presione cualquier tecla para cerrar...");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("✔️  BitLocker eliminado correctamente.");
            }
            else
            {
                Console.WriteLine("Unidad no cifrada con BitLocker, saltando paso de descifrado...");
            }

            // Borrar todo el contenido de la unidad
            try
            {
                DirectoryInfo di = new DirectoryInfo(drive.RootDirectory.FullName);
                foreach (FileInfo file in di.GetFiles()) file.Delete();
                foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                Console.WriteLine("✔️  Unidad limpiada correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("✖  Error al limpiar la unidad: " + ex.Message);
                Console.WriteLine("Presione cualquier tecla para cerrar...");
                Console.ReadKey();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Unidad lista para reutilizar.\nVuelve a ejecutar el bootstrap para crear un nuevo USB root.");
            Console.ResetColor();
            Console.WriteLine("Presione cualquier tecla para cerrar...");
            Console.ReadKey();
            return;
        }
        else if (choice == "2")
        {
            // FLUJO NORMAL (CREAR USB ROOT)
            const string apiBase = "https://192.168.1.209:8443";   // ← URL/IP del backend
            Debug.WriteLine($"▲ bootstrap iniciado  –  API base: {apiBase}");

            // 0) Datos del usuario root-admin
            string Ask(string label, string hint, string def)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{label} ");
                Console.ResetColor();
                Console.Write($"[{hint}]  (Enter = \"{def}\"): ");
                string? v = Console.ReadLine();
                return string.IsNullOrWhiteSpace(v) ? def : v.Trim();
            }

            string rut = Ask("RUT         :", "idealmente Jefe de TI", "11.111.111-1");
            string nombre = Ask("Nombre      :", "nombre de la empresa", "admin");
            string depto = Ask("Departamento:", "root / TI", "TI");
            string email = Ask("Email       :", "correo corporativo", "admin@empresa.cl");
            string pin = Ask("PIN         :", "un número inolvidable", "1234");

            // 1) Detectar edición de Windows y advertir si BitLocker no está disponible
            string edition = BootstrapHelpers.GetWindowsEdition();
            bool canBitLocker =
                edition.IndexOf("pro", StringComparison.OrdinalIgnoreCase) >= 0 ||
                edition.IndexOf("enterprise", StringComparison.OrdinalIgnoreCase) >= 0 ||
                edition.IndexOf("education", StringComparison.OrdinalIgnoreCase) >= 0 ||
                edition.IndexOf("server", StringComparison.OrdinalIgnoreCase) >= 0;

            bool warningMode = false;
            if (!canBitLocker)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️  ADVERTENCIA: Estás en Windows " + edition + ". Sólo Windows Pro, Enterprise o Education permiten cifrar unidades con BitLocker.");
                Console.WriteLine("Si continúas sin cifrar, las llaves y datos críticos en el USB NO estarán protegidos en caso de robo o pérdida.");
                Console.WriteLine("¿Deseas continuar SIN cifrado BitLocker? (S/N):");
                Console.ResetColor();

                var resp = Console.ReadLine()?.Trim().ToUpper();
                if (resp != "S")
                {
                    warningMode = true;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("✔️  BitLocker disponible: se recomienda cifrar el USB root para máxima seguridad.");
                Console.ResetColor();
            }

            // 2) Pedir la clave BitLocker (sólo si es posible cifrar)
            string? bitlockerKey = null;
            if (canBitLocker)
            {
                while (true)
                {
                    Console.WriteLine("\nClave BitLocker para cifrar el USB root (mínimo 8 caracteres, recomendable fuerte):");
                    bitlockerKey = Console.ReadLine()?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(bitlockerKey) && bitlockerKey.Length >= 8)
                        break;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("La clave es obligatoria y debe tener al menos 8 caracteres. Intente de nuevo.");
                    Console.ResetColor();
                }
            }

            Debug.WriteLine($"► Seleccionado: {drive.Name}  {drive.TotalSize / 1_073_741_824} GB");

            string serial = UsbHelper.GetUsbSerial(drive.Name.TrimEnd('\\'));
            Debug.WriteLine($"Serial obtenido: {serial}");
            if (serial is "UNKNOWN" or "")
            {
                Console.WriteLine("✖  Serial USB desconocido; abortando.");
                Console.WriteLine("Presione cualquier tecla para cerrar...");
                Console.ReadKey();
                return;
            }

            // 3) Cifrado BitLocker (si corresponde) y almacenamiento seguro de clave
            if (canBitLocker && bitlockerKey != null)
            {
                if (!BitLockerManager.EncryptDrive(drive.Name.TrimEnd('\\'), bitlockerKey))
                {
                    Console.WriteLine("✖ Error al cifrar la unidad con BitLocker.");
                    Console.WriteLine("Presione cualquier tecla para cerrar...");
                    Console.ReadKey();
                    return;
                }

                //Mostrar Porgreso
                BootstrapHelpers.ShowBitLockerProgressCmd(drive.Name.TrimEnd('\\'));

                // Guardar la clave en el USB root, cifrada con sí misma
                byte[] encryptedKey = CryptoHelper.EncryptString(bitlockerKey, bitlockerKey);
                string sysDir = Path.Combine(drive.RootDirectory.FullName, "rusbp.sys");
                Directory.CreateDirectory(sysDir);
                File.WriteAllBytes(Path.Combine(sysDir, ".btlk"), encryptedKey);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✔️  USB root cifrado y clave protegida.");
                Console.ResetColor();
            }
            else if (warningMode || !canBitLocker)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️  El USB root será creado SIN cifrado BitLocker. Esto es INSEGURO y NO RECOMENDADO.");
                Console.ResetColor();
            }

            // 4) HttpClient (omitir TLS sólo para la IP del backend)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errs) =>
                    req.RequestUri!.Host == new Uri(apiBase).Host
            };
            using var http = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };

            // 5) Crear / registrar el USB
            var usbDto = new { serial, thumbprint = "" };
            var usbResp = await http.PostAsJsonAsync("/api/usb", usbDto);

            if (!usbResp.IsSuccessStatusCode)
            {
                var errorBody = await usbResp.Content.ReadAsStringAsync();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✖ Error HTTP {(int)usbResp.StatusCode}: {usbResp.StatusCode}");
                Console.WriteLine($"Detalle: {errorBody}");
                Console.ResetColor();
                Console.WriteLine("Presione cualquier tecla para cerrar...");
                Console.ReadKey();
                return;
            }

            Debug.WriteLine($"Respuesta HTTP: {(int)usbResp.StatusCode} {usbResp.StatusCode}");

            // 6) Crear usuario
            var userDto = new
            {
                rut,
                nombre,
                depto,
                email,
                rol = "Admin",
                pin
            };
            Debug.WriteLine("POST /api/usuarios  →  " + JsonSerializer.Serialize(userDto));
            var usrResp = await http.PostAsJsonAsync("/api/usuarios", userDto);
            Debug.WriteLine($"Respuesta HTTP: {(int)usrResp.StatusCode} {usrResp.StatusCode}");
            usrResp.EnsureSuccessStatusCode();
            var usuario = await usrResp.Content.ReadFromJsonAsync<UsuarioCreated>();
            Debug.WriteLine($"Usuario creado.  id={usuario!.id}  msg={usuario.msg}");

            // 7) Vincular USB ⇄ Usuario
            var vincDto = new { serial, usuarioRut = rut };
            Debug.WriteLine("POST /api/usb/asignar  →  " + JsonSerializer.Serialize(vincDto));
            var vincResp = await http.PostAsJsonAsync("/api/usb/asignar", vincDto);
            Debug.WriteLine($"Respuesta HTTP: {(int)vincResp.StatusCode} {vincResp.StatusCode}");
            vincResp.EnsureSuccessStatusCode();

            // 8) Generar PKI en el pendrive
            var pkiDir = Path.Combine(drive.RootDirectory.FullName, "pki");
            Directory.CreateDirectory(pkiDir);
            Debug.WriteLine($"Generando PKI en {pkiDir} …");
            var (certPath, keyPath) = PkiService.GeneratePkcs8KeyPair(serial, pkiDir);
            Debug.WriteLine($"Cert   → {certPath}");
            Debug.WriteLine($"Clave  → {keyPath}");

            // 9) config.json
            var cfg = new
            {
                nombre,
                rut,
                email,
                rol = "Admin",
                Serial = serial,
                Fecha = DateTime.UtcNow
            };
            string cfgPath = Path.Combine(drive.RootDirectory.FullName, "config.json");
            File.WriteAllText(cfgPath,
                JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
            Debug.WriteLine($"config.json escrito en {cfgPath}");

            // Éxito final
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nUSB-ADM preparado ✔️  ¡Arranca la aplicación!");
            Console.ResetColor();
            Console.WriteLine("Presione cualquier tecla para cerrar...");
            Console.ReadKey();
            return;
        }
        else
        {
            Console.WriteLine("Opción no válida. Intente de nuevo.");
            continue;
        }
    }
}

// ═════════════════════════════════════════════════════════════════════
//  Helpers & Modelos
// ═════════════════════════════════════════════════════════════════════

record UsuarioCreated(int id, string? msg);

// BitLocker helpers
static class BitLockerManager
{
    public static bool EncryptDrive(string driveLetter, string password)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"Enable-BitLocker -MountPoint '{driveLetter}' -Password (ConvertTo-SecureString '{password}' -AsPlainText -Force) -PasswordProtector -EncryptionMethod XtsAes128\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        var process = Process.Start(psi);
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine("=== BitLocker STDOUT ===");
        Console.WriteLine(output);
        if (!string.IsNullOrWhiteSpace(error))
        {
            Console.WriteLine("=== BitLocker STDERR ===");
            Console.WriteLine(error);
        }
        Console.WriteLine("========================");

        return process.ExitCode == 0;
    }
}

// Cifrado AES simple usando la propia clave
static class CryptoHelper
{
    public static byte[] EncryptString(string plainText, string password)
    {
        using var aes = Aes.Create();
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32);
        aes.Key = key;
        aes.GenerateIV();
        using var ms = new MemoryStream();
        ms.Write(salt); // 16 bytes
        ms.Write(aes.IV); // 16 bytes
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
            sw.Write(plainText);
        return ms.ToArray();
    }
}

// ---------- UsbHelper ------------------------------------------------
static class UsbHelper
{
    public static string GetUsbSerial(string driveLetter /* «F» ó «F:\» *//*)
    {
        driveLetter = driveLetter.TrimEnd('\\', ':');

        try
        {
            using var q = new ManagementObjectSearcher(
                $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}:'}} " +
                "WHERE AssocClass=Win32_LogicalDiskToPartition");

            foreach (ManagementObject part in q.Get())
            {
                using var disks = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{part["DeviceID"]}'}} " +
                    "WHERE AssocClass=Win32_DiskDriveToDiskPartition");

                foreach (ManagementObject disk in disks.Get())
                {
                    var sn = (disk["SerialNumber"]?.ToString() ?? "").Trim();
                    if (!string.IsNullOrWhiteSpace(sn))
                    {
                        Debug.WriteLine($"  WMI  → {sn}");
                        return sn.ToUpperInvariant();
                    }
                }
            }
        }
        catch (Exception ex) { Debug.WriteLine("WMI error: " + ex.Message); }

        // Volcado de volumen si WMI falla
        if (GetVolumeInformation($@"{driveLetter}:\",
                null, 0, out uint volSer, out _, out _, null, 0))
            return volSer.ToString("X8");

        return "UNKNOWN";
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetVolumeInformation(
        string lpRootPathName,
        StringBuilder? lpVolumeNameBuffer,
        int nVolumeNameSize,
        out uint lpVolumeSerialNumber,
        out uint lpMaximumComponentLength,
        out uint lpFileSystemFlags,
        StringBuilder? lpFileSystemNameBuffer,
        int nFileSystemNameSize);
}

// ---------- PkiService -----------------------------------------------
static class PkiService
{
    public static (string certPath, string keyPath) GeneratePkcs8KeyPair(
        string commonName, string destDir)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            new X500DistinguishedName($"CN={commonName}"),
            rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow,
                                              DateTimeOffset.UtcNow.AddYears(5));

        string certPath = Path.Combine(destDir, "cert.crt");
        string keyPath = Path.Combine(destDir, "priv.key");

        File.WriteAllText(certPath,
            PemEncoding.Write("CERTIFICATE", cert.RawData));

        File.WriteAllText(keyPath,
            PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey()));

        return (certPath, keyPath);
    }
}

static class BootstrapHelpers
{
    public static string GetWindowsEdition()
    {
        using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
        {
            var edition = key?.GetValue("EditionID")?.ToString();
            return edition ?? "UNKNOWN";
        }
    }
    public static void ShowBitLockerProgressCmd(string driveLetter)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n[INFO] Monitoreando el progreso de cifrado de la unidad {driveLetter}: ...");
        Console.ResetColor();

        while (true)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "manage-bde.exe",
                Arguments = $"-status {driveLetter}:",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Buscar la línea del porcentaje (soporta español e inglés)
            string percentStr = null;
            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("Porcentaje cifrado:") || line.Contains("Percentage Encrypted:"))
                {
                    percentStr = line.Split(':').LastOrDefault()?.Trim().Replace("%", "");
                    break;
                }
            }

            if (percentStr != null && double.TryParse(percentStr, out double percent))
            {
                Console.Write($"\rCifrado BitLocker: {percent}% completado...    ");
                if (percent >= 100.0) break;
            }
            else
            {
                Console.Write($"\rCifrado BitLocker: progreso desconocido...    ");
            }

            System.Threading.Thread.Sleep(5000); // Espera 5 segundos
        }
        Console.WriteLine("\nCifrado BitLocker completado.");
    }
}

public static class BitLockerHelpers
{
    public static bool UnlockDrive(string driveLetter, string password)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"Unlock-BitLocker -MountPoint '{driveLetter}' -Password (ConvertTo-SecureString '{password}' -AsPlainText -Force)\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        var process = Process.Start(psi);
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Console.WriteLine(output);
        if (!string.IsNullOrWhiteSpace(error)) Console.WriteLine(error);
        return process.ExitCode == 0;
    }

    public static bool DecryptDrive(string driveLetter)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-Command \"Disable-BitLocker -MountPoint '{driveLetter}'\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        var process = Process.Start(psi);
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Console.WriteLine(output);
        if (!string.IsNullOrWhiteSpace(error)) Console.WriteLine(error);
        return process.ExitCode == 0;
    }

}
*/
