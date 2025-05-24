using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace rusbp_bootstrap.Core
{
    public static class BitLockerManager
    {
        // Cifrado de unidad
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
            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine(error);
                if (error.Contains("BitLocker no está habilitado") || error.Contains("not enabled on this drive"))
                {
                    // No es error fatal, la unidad ya no está cifrada
                    return true;
                }
            }
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
    }
}
