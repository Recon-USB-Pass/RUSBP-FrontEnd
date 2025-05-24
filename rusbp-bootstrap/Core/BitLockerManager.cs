using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace rusbp_bootstrap.Core
{
    public static class BitLockerManager
    {
        // Cifrar la unidad
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

        // Progreso de cifrado (polling)
        public static void ShowBitLockerProgress(string driveLetter)
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

                Thread.Sleep(5000); // 5 segundos
            }
            Console.WriteLine("\nCifrado BitLocker completado.");
        }

        // Verifica si la unidad está bloqueada
        public static bool IsDriveLocked(string driveLetter)
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
            // Busca “Estado de bloqueo” o “Lock Status”
            return output.Contains("Bloqueado") || output.Contains("Locked");
        }

        // Desbloquear unidad
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

        // Bloquear unidad (opcional)
        public static bool LockDrive(string driveLetter)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "manage-bde.exe",
                Arguments = $"-lock {driveLetter}: -forcedismount",
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

        // Descifrar unidad
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

        // Limpieza "best effort" ignorando archivos de BitLocker
        public static void LimpiarUnidadBestEffort(string driveLetter)
        {
            var rootPath = driveLetter + @":\";
            DirectoryInfo di = new DirectoryInfo(rootPath);
            int totalFiles = 0, deletedFiles = 0, failedFiles = 0, ignoredBitLocker = 0;
            int totalDirs = 0, deletedDirs = 0, failedDirs = 0;

            foreach (FileInfo file in di.GetFiles())
            {
                totalFiles++;
                try
                {
                    // Ignorar archivos de BitLocker (ej. FVE2.*)
                    if (file.Name.StartsWith("FVE", StringComparison.OrdinalIgnoreCase))
                    {
                        ignoredBitLocker++;
                        continue;
                    }
                    file.Delete();
                    deletedFiles++;
                }
                catch (Exception ex)
                {
                    failedFiles++;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"No se pudo borrar archivo '{file.Name}': {ex.Message}");
                    Console.ResetColor();
                }
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                totalDirs++;
                try
                {
                    dir.Delete(true);
                    deletedDirs++;
                }
                catch (Exception ex)
                {
                    failedDirs++;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"No se pudo borrar carpeta '{dir.Name}': {ex.Message}");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Limpieza completada: {deletedFiles}/{totalFiles} archivos, {deletedDirs}/{totalDirs} carpetas borradas correctamente.");
            if (ignoredBitLocker > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Se detectaron {ignoredBitLocker} archivos protegidos de BitLocker, que no pueden ser borrados (esto es normal).");
            }
            if (failedFiles > 0 || failedDirs > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Algunos archivos/carpetas no pudieron ser borrados (quizás en uso o protegidos por el sistema).");
            }
            Console.ResetColor();
        }
    }
}
