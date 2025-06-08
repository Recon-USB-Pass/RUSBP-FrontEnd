// RUSBP_Admin.Core.Helpers.DriveEncryption.cs
// -------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Helpers
{
    /// <summary>Helper BitLocker – cifra USB y recupera la RecoveryPassword.</summary>
    public static class DriveEncryption
    {
        /// <summary>
        /// Cifra la unidad y devuelve la RecoveryPassword (RP_x).
        /// </summary>
        /// <param name="mountPoint">“F:\” o “F:”</param>
        /// <param name="progressCallback">callback con el %</param>
        public static async Task<string> EncryptDriveWithBitLockerAsync(
            string mountPoint,
            Action<int>? progressCallback = null)
        {
            string drive = mountPoint.TrimEnd('\\', ':') + ":";

            // 1️⃣   Habilitar BitLocker (protector numérico + «-UsedSpaceOnly»)
            var enable = RunCmd("manage-bde", $"-on {drive} -RecoveryPassword -UsedSpaceOnly");
            if (!enable.Success)
                throw new Exception("Falló el comando BitLocker –on:\n" + enable.Output);

            // 2️⃣   Esperar progreso de cifrado
            int percent = 0;
            int failStall = 0;

            while (percent < 100)
            {
                string statusOut = RunCmd("manage-bde", $"-status {drive}").Output;

                // a) porcentaje si existe
                percent = ParsePercentage(statusOut);
                // b) si sigue 0 % comprobar “Cifrado completo / Fully Encrypted”
                if (percent == 0 && ConversionComplete(statusOut))
                    percent = 100;

                progressCallback?.Invoke(Math.Min(percent, 99));   // evita saltar 100 demasiado pronto

                // c) control de estancamiento
                if (percent == 0)
                {
                    if (++failStall > 20)   // ~30 s sin avance real
                        throw new Exception("BitLocker no reporta avance de cifrado.");
                }
                else failStall = 0;

                if (percent < 100) await Task.Delay(1500);
            }
            progressCallback?.Invoke(100);

            // 3️⃣   Bloquear si quedó abierto
            if (!IsLocked(drive))
            {
                var l = RunCmd("manage-bde", $"-lock {drive} -ForceDismount");
                if (!l.Success)
                    throw new Exception("No se pudo bloquear la unidad tras el cifrado:\n" + l.Output);
            }

            // 4️⃣   Obtener la RP asignada en este ciclo
            var getProt = RunCmd("manage-bde", $"-protectors {drive} -get");
            string rp = ExtractRecoveryPassword(getProt.Output);
            if (string.IsNullOrWhiteSpace(rp))
                throw new Exception("No se pudo extraer la RecoveryPassword:\n" + getProt.Output);

            return rp;
        }

        // ═════════════════════════ UTILIDADES ═════════════════════════

        private static int ParsePercentage(string statusOut)
        {
            var m = Regex.Match(statusOut,
                @"(Porcentaje cifrado|Encryption Percentage):\s*([\d\.]+)%", RegexOptions.IgnoreCase);
            return m.Success && int.TryParse(m.Groups[2].Value.Split('.')[0], out var p) ? p : 0;
        }

        private static bool ConversionComplete(string statusOut) =>
            statusOut.IndexOf("Cifrado completo", StringComparison.OrdinalIgnoreCase) >= 0 ||
            statusOut.IndexOf("Fully Encrypted", StringComparison.OrdinalIgnoreCase) >= 0 ||
            statusOut.IndexOf("Conversion Status:    Fully Encrypted", StringComparison.OrdinalIgnoreCase) >= 0;

        public static bool IsLocked(string drive)
        {
            var st = RunCmd("manage-bde", $"-status {drive}").Output;
            return st.Contains("Bloqueado", StringComparison.OrdinalIgnoreCase) ||
                   st.Contains("Locked", StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractRecoveryPassword(string output)
        {
            var m = Regex.Match(output, @"\b\d{6}(?:-\d{6}){7}\b");
            return m.Success ? m.Value : "";
        }

        private static (bool Success, string Output) RunCmd(string exe, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exe,
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
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
