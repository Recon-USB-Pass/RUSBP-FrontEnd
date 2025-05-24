using rusbp_bootstrap.Core;
using rusbp_bootstrap.Api;
using rusbp_bootstrap.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static async Task Main()
    {
        UsbDeviceInfo? selectedUsb = null;

        while (true)
        {
            if (selectedUsb == null)
            {
                Console.Clear();
                selectedUsb = UsbManager.SelectUsbDevice();
                Console.Clear();
            }

            // Mostrar siempre la unidad activa al tope
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Unidad en uso: {selectedUsb.DriveLetter ?? "(sin letra)"} - Serial: {selectedUsb.Serial} - {selectedUsb.VolumeLabel ?? ""} - {(selectedUsb.SizeBytes ?? 0) / 1024 / 1024 / 1024} GB");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n⚠️  No retire la unidad seleccionada hasta que el proceso finalice.");
            Console.ResetColor();

            // Flujo recomendado (verde)
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nFlujo recomendado:");
            Console.WriteLine("1) Seleccione la unidad USB a trabajar.");
            Console.WriteLine("2) Luego, LIMPIE la unidad si viene de un uso anterior.");
            Console.WriteLine("3) Si desea, DESCIFRE la unidad (si está cifrada y quiere quitar BitLocker).");
            Console.WriteLine("4) CIFRE la unidad con BitLocker si aún no está cifrada.");
            Console.WriteLine("5) Finalmente, REGISTRE el root (esto solo asocia la unidad/usuario y genera llaves).");
            Console.WriteLine("Si la unidad ya está cifrada, puede saltar directo al registro, se le pedirá la clave BitLocker solo cuando sea necesario.");
            Console.ResetColor();

            // Menú principal
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nSeleccione una opción:");
            Console.WriteLine("1. Cambiar unidad USB");
            Console.WriteLine("2. Limpiar la unidad (BORRAR TODO, NO descifra)");
            Console.WriteLine("3. Descifrar la unidad (quita BitLocker, no borra archivos)");
            Console.WriteLine("4. Cifrar unidad con BitLocker (solo si aún no está cifrada)");
            Console.WriteLine("5. Registrar root (no cifra, solo registra backend y genera PKI/config)");
            Console.WriteLine("6. Salir");
            Console.ResetColor();

            var menuOpt = Console.ReadLine()?.Trim();
            bool showSuccess = false, showFail = false;
            string? resultMsg = null;

            if (menuOpt == "6")
                break;

            // Cambiar unidad USB
            if (menuOpt == "1")
            {
                selectedUsb = null;
                Console.Clear();
                continue;
            }

            // Recordatorio de unidad activa
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n[INFO] Trabajando sobre: {selectedUsb.DriveLetter ?? "(sin letra)"} - Serial: {selectedUsb.Serial}");
            Console.ResetColor();

            // Limpiar unidad
            if (menuOpt == "2")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚠️  Esta acción borrará TODOS los archivos de la unidad (NO descifra). ¿Desea continuar? (S/N): ");
                Console.ResetColor();
                var resp = Console.ReadLine()?.Trim().ToUpper();
                if (resp != "S") { Console.Clear(); continue; }

                bool isLocked = BitLockerManager.IsDriveLocked(selectedUsb.DriveLetter);
                bool wasUnlocked = false;
                string? claveBitlocker = null;

                if (isLocked)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("La unidad está bloqueada con BitLocker. Se requiere la clave para limpiarla.");
                    Console.ResetColor();
                    Console.Write("Ingrese la clave BitLocker de la unidad: ");
                    claveBitlocker = Console.ReadLine()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(claveBitlocker))
                    {
                        resultMsg = "No se ingresó clave. Abortando limpieza.";
                        showFail = true;
                        goto EndOfAction;
                    }
                    if (!BitLockerManager.UnlockDrive(selectedUsb.DriveLetter, claveBitlocker))
                    {
                        resultMsg = "✖  No se pudo desbloquear la unidad. Verifique la clave.";
                        showFail = true;
                        goto EndOfAction;
                    }
                    wasUnlocked = true;
                }

                // Limpiar archivos de la unidad
                try
                {
                    DirectoryInfo di = new DirectoryInfo(selectedUsb.DriveLetter + @":\");
                    int totalFiles = 0, deletedFiles = 0, failedFiles = 0;
                    int totalDirs = 0, deletedDirs = 0, failedDirs = 0;

                    foreach (FileInfo file in di.GetFiles())
                    {
                        totalFiles++;
                        try { file.Delete(); deletedFiles++; }
                        catch { failedFiles++; }
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        totalDirs++;
                        try { dir.Delete(true); deletedDirs++; }
                        catch { failedDirs++; }
                    }

                    resultMsg = $"Limpieza completada: {deletedFiles}/{totalFiles} archivos, {deletedDirs}/{totalDirs} carpetas borradas correctamente.";
                    if (failedFiles > 0 || failedDirs > 0)
                        resultMsg += "\nAlgunos archivos/carpetas no pudieron ser borrados (quizás en uso o protegidos por el sistema).";
                    showSuccess = true;
                }
                catch (Exception ex)
                {
                    resultMsg = $"✖  Error al limpiar la unidad: {ex.Message}";
                    showFail = true;
                }

                if (wasUnlocked && !string.IsNullOrWhiteSpace(claveBitlocker))
                {
                    if (BitLockerManager.LockDrive(selectedUsb.DriveLetter))
                        resultMsg += "\n🔒 Unidad bloqueada nuevamente con BitLocker.";
                    else
                        resultMsg += "\n⚠️  No se pudo bloquear la unidad automáticamente, hágalo manualmente.";
                }

                goto EndOfAction;
            }

            // Descifrar unidad
            if (menuOpt == "3")
            {
                Console.WriteLine("Ingrese la clave BitLocker de la unidad:");
                string clave = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(clave))
                {
                    resultMsg = "No se ingresó clave. Abortando descifrado.";
                    showFail = true;
                    goto EndOfAction;
                }
                if (!BitLockerManager.UnlockDrive(selectedUsb.DriveLetter, clave))
                {
                    resultMsg = "✖  No se pudo desbloquear la unidad. Verifique la clave.";
                    showFail = true;
                    goto EndOfAction;
                }
                if (!BitLockerManager.DecryptDrive(selectedUsb.DriveLetter))
                {
                    resultMsg = "✖  No se pudo descifrar la unidad con BitLocker.";
                    showFail = true;
                    goto EndOfAction;
                }
                resultMsg = "✔️  BitLocker eliminado correctamente.";
                showSuccess = true;
                goto EndOfAction;
            }

            // Cifrar unidad
            if (menuOpt == "4")
            {
                var edition = BootstrapHelpers.GetWindowsEdition();
                bool canBitLocker =
                    edition.IndexOf("pro", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    edition.IndexOf("enterprise", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    edition.IndexOf("education", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    edition.IndexOf("server", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!canBitLocker)
                {
                    resultMsg = $"⚠️  Estás en {edition}. Solo Pro, Enterprise o Education permiten cifrar con BitLocker.";
                    showFail = true;
                    goto EndOfAction;
                }
                string? bitlockerKey = null;
                while (true)
                {
                    Console.WriteLine("\nClave BitLocker para cifrar el USB root (mínimo 8 caracteres):");
                    bitlockerKey = Console.ReadLine()?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(bitlockerKey) && bitlockerKey.Length >= 8)
                        break;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("La clave debe tener al menos 8 caracteres. Intente de nuevo.");
                    Console.ResetColor();
                }

                if (!BitLockerManager.EncryptDrive(selectedUsb.DriveLetter, bitlockerKey))
                {
                    resultMsg = "✖ Error al cifrar la unidad con BitLocker.";
                    showFail = true;
                    goto EndOfAction;
                }
                BitLockerManager.ShowBitLockerProgress(selectedUsb.DriveLetter);

                // Guardar la clave cifrada en el propio USB
                byte[] encryptedKey = CryptoHelper.EncryptString(bitlockerKey, bitlockerKey);
                string sysDir = Path.Combine(selectedUsb.DriveLetter + @":\", "rusbp.sys");
                Directory.CreateDirectory(sysDir);
                File.WriteAllBytes(Path.Combine(sysDir, ".btlk"), encryptedKey);

                resultMsg = "✔️  Unidad cifrada y clave protegida.";
                showSuccess = true;
                goto EndOfAction;
            }

            // Registrar root (registra backend, PKI/config, TESTEA login/verify, elimina si falla)
            if (menuOpt == "5")
            {
                // --- 1. Desbloquear si está cifrada ---
                bool isLocked = BitLockerManager.IsDriveLocked(selectedUsb.DriveLetter);
                if (isLocked)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("La unidad está cifrada con BitLocker. Se requiere la clave para registrar root y escribir archivos.");
                    Console.ResetColor();

                    while (true)
                    {
                        Console.Write("Ingrese la clave BitLocker de la unidad (ENTER para cancelar): ");
                        string claveBitlocker = Console.ReadLine()?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(claveBitlocker))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("No se ingresó clave. Abortando registro root.");
                            Console.ResetColor();
                            break;
                        }
                        if (!BitLockerManager.UnlockDrive(selectedUsb.DriveLetter, claveBitlocker))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("✖ No se pudo desbloquear la unidad. Verifique la clave e intente de nuevo.");
                            Console.ResetColor();
                            continue;
                        }
                        break;
                    }
                }

                // *** GENERA PKI Y OBTIENE THUMBPRINT ANTES DE REGISTRAR EL USB ***
                var pkiDir = Path.Combine(selectedUsb.DriveLetter + @":\", "pki");
                Directory.CreateDirectory(pkiDir);
                var (certPath, keyPath) = PkiService.GeneratePkcs8KeyPair(selectedUsb.Serial, pkiDir);

                // Leer el thumbprint SHA1 del certificado recién generado
                var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath);
                string thumbprint = cert.Thumbprint?.ToUpperInvariant() ?? "";

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, certH, chain, errs) => true // SOLO DEV
                };
                using var http = new HttpClient(handler) { BaseAddress = new Uri("https://192.168.1.209:8443") };
                var backend = new BackendClient(http);

                var usuario = new Usuario
                {
                    Rut = BootstrapHelpers.Prompt("RUT         :", "idealmente Jefe de TI", "11.111.111-1"),
                    Nombre = BootstrapHelpers.Prompt("Nombre      :", "nombre de la empresa", "admin"),
                    Depto = BootstrapHelpers.Prompt("Departamento:", "root / TI", "TI"),
                    Email = BootstrapHelpers.Prompt("Email       :", "correo corporativo", "admin@empresa.cl"),
                    Rol = "Admin",
                    Pin = BootstrapHelpers.Prompt("PIN         :", "un número inolvidable", "1234")
                };

                // REGISTRA EL USB CON EL THUMBPRINT YA OBTENIDO
                var okUsb = await backend.RegistrarUsbAsync(selectedUsb.Serial, thumbprint);
                if (!okUsb)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✖ Error al registrar el USB en backend.");
                    Console.ResetColor();
                    continue;
                }

                var userDto = new UsuarioDto
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✖ Error al crear usuario en backend.");
                    Console.ResetColor();
                    await backend.EliminarUsbAsync(selectedUsb.Serial);
                    continue;
                }

                var okVinc = await backend.AsignarUsbAUsuarioAsync(selectedUsb.Serial, usuario.Rut);
                if (!okVinc)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✖ Error al asociar USB y usuario en backend.");
                    Console.ResetColor();
                    await backend.EliminarUsuarioAsync(usuario.Rut);
                    await backend.EliminarUsbAsync(selectedUsb.Serial);
                    continue;
                }

                // === PRUEBAS AUTENTICACIÓN USB ===
                // 1. Leer certificado y clave privada PEM generados
                string certPem = File.ReadAllText(certPath);
                string privKeyPem = File.ReadAllText(keyPath);

                // 2. Obtener challenge del backend
                string? challengeB64 = await backend.ObtenerChallengeVerifyUsbAsync(selectedUsb.Serial, certPem);
                if (string.IsNullOrWhiteSpace(challengeB64))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ No se pudo obtener el challenge de verify-usb. Rollback.");
                    Console.ResetColor();
                    await backend.EliminarUsuarioAsync(usuario.Rut);
                    await backend.EliminarUsbAsync(selectedUsb.Serial);
                    continue;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✔️  [BACKEND] /api/auth/verify-usb exitoso. Challenge recibido.");
                    Console.ResetColor();
                }

                // 3. Firmar el challenge usando la clave privada del USB
                string signatureB64;
                try
                {
                    signatureB64 = CryptoHelper.FirmarChallenge(privKeyPem, challengeB64);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Error al firmar el challenge: " + ex.Message);
                    Console.ResetColor();
                    await backend.EliminarUsuarioAsync(usuario.Rut);
                    await backend.EliminarUsbAsync(selectedUsb.Serial);
                    continue;
                }

                // 4. Probar login usando la firma y el PIN (y una MAC dummy por ahora)
                bool loginOk = await backend.ProbarLoginAsync(selectedUsb.Serial, signatureB64, usuario.Pin, "00-11-22-33-44-55");

                if (!loginOk)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ [BACKEND] /api/auth/login falló. Eliminando usuario y USB (rollback).");
                    Console.ResetColor();
                    await backend.EliminarUsuarioAsync(usuario.Rut);
                    await backend.EliminarUsbAsync(selectedUsb.Serial);
                    continue;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✔️  [BACKEND] /api/auth/login exitoso. ¡Login test ok!");
                    Console.ResetColor();
                }


                // Crear config.json
                var config = new ConfigJson
                {
                    Nombre = usuario.Nombre,
                    Rut = usuario.Rut,
                    Email = usuario.Email,
                    Rol = "Admin",
                    Serial = selectedUsb.Serial,
                    Fecha = DateTime.UtcNow
                };
                string cfgPath = Path.Combine(selectedUsb.DriveLetter + @":\", "config.json");
                File.WriteAllText(cfgPath, System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nUSB-ADM preparado ✔️  ¡Arranca la aplicación!");
                Console.ResetColor();

                continue;
            }


        EndOfAction:
            Console.Clear();
            if (showSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(resultMsg ?? "✅ Operación exitosa");
                Console.ResetColor();
            }
            else if (showFail)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(resultMsg ?? "❌ Ocurrió un error");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(resultMsg ?? "");
            }
            // Mostrar unidad activa antes del menú
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nUnidad en uso: {selectedUsb.DriveLetter ?? "(sin letra)"} - Serial: {selectedUsb.Serial}");
            Console.ResetColor();
        }
        Console.WriteLine("\nPresione cualquier tecla para cerrar...");
        Console.ReadKey();
    }
}
