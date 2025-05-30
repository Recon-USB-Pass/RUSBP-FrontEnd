using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Helpers
{
    public static class DriveEncryption
    {
        /// <summary>
        /// Cifra la unidad con BitLocker y retorna la RecoveryPassword generada.
        /// </summary>
        public static async Task<string> EncryptDriveWithBitLockerAsync(string mountPoint, Action<int>? progressCallback = null)
        {
            string drive = mountPoint.TrimEnd('\\', ':') + ":";

            // Habilita BitLocker con protector de recuperación
            var enable = RunCmd("manage-bde.exe", $"-on {drive} -RecoveryPassword -UsedSpaceOnly");
            if (!enable.Success)
                throw new Exception("Falló el comando de cifrado BitLocker:\n" + enable.Output);

            // Esperar progreso de cifrado
            int percent = 0;
            while (percent < 100)
            {
                percent = GetEncryptionPercentage(drive);
                progressCallback?.Invoke(percent);
                await Task.Delay(1200);
            }

            // Obtener RecoveryPassword
            var result = RunCmd("manage-bde.exe", $"-protectors {drive} -get");
            string rp = ExtraerRecoveryPassword(result.Output);
            if (string.IsNullOrWhiteSpace(rp))
                throw new Exception("No se pudo extraer RecoveryPassword.");

            return rp;
        }

        /// <summary>
        /// Extrae el porcentaje de cifrado de BitLocker.
        /// </summary>
        public static int GetEncryptionPercentage(string drive)
        {
            var result = RunCmd("manage-bde.exe", $"-status {drive}");
            var match = Regex.Match(result.Output, @"Encryption Percentage:\s*(\d+)%");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int percent))
                return percent;
            return 0;
        }

        /// <summary>
        /// Extrae el RecoveryPassword desde el output de manage-bde.
        /// </summary>
        private static string ExtraerRecoveryPassword(string output)
        {
            var match = Regex.Match(output, @"\b\d{6}(?:-\d{6}){7}\b");
            return match.Success ? match.Value : "";
        }

        /// <summary>
        /// Ejecuta un comando externo y captura salida + error.
        /// </summary>
        private static (bool Success, string Output) RunCmd(string file, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = Process.Start(psi)!;
                string output = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                p.WaitForExit();
                return (p.ExitCode == 0, output);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
