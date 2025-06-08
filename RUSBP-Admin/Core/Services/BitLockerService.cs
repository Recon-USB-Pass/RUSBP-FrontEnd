using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RUSBP_Admin.Core.Services
{
    /// <summary>
    /// Métodos utilitarios para gestionar el estado de unidades BitLocker (bloqueo, desbloqueo, verificación).
    /// Se usa en el flujo de preparación de USBs administrados y empleados.
    /// </summary>
    public static class BitLockerService
    {
        /// <summary>
        /// Intenta desbloquear la unidad con la contraseña de usuario (NO RecoveryPassword).
        /// </summary>
        public static void TryUnlock(string letter, string password)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe",
                    $"/c manage-bde -unlock {letter.TrimEnd('\\')} -password {password} >nul 2>&1")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(8000);
                // Logging opcional, útil para debug avanzado
                // LoggingService.Debug($"Unlock {letter} exit={p?.ExitCode}");
            }
            catch (Exception ex)
            {
                // Logging opcional: LoggingService.Debug($"Unlock error {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si la unidad está actualmente bloqueada por BitLocker.
        /// </summary>
        public static bool IsLocked(string driveLetter)
        {
            try
            {
                string dl = driveLetter.Trim().TrimEnd('\\').TrimEnd(':') + ":";

                var psi = new ProcessStartInfo
                {
                    FileName = "manage-bde.exe",
                    Arguments = $"-status {dl}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi)!;
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(3000);

                // Busca línea relevante (multiidioma si deseas ampliar soporte)
                var match = Regex.Match(output, @"Estado de bloqueo:\s+(Bloqueado|Desbloqueado)", RegexOptions.IgnoreCase);
                if (!match.Success) return false; // Si no encuentra, asumimos desbloqueado

                return match.Groups[1].Value.StartsWith("Bloqueado", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // En caso de excepción, asumimos desbloqueado para evitar UI blockeos inesperados.
                return false;
            }
        }
        public static bool IsBitLockerUnlocked(string driveLetter)
        {
            try
            {
                string dl = driveLetter.Trim().TrimEnd('\\').TrimEnd(':') + ":";
                var p = Process.Start(new ProcessStartInfo
                {
                    FileName = "manage-bde.exe",
                    Arguments = $"-status {dl}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                })!;
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(3000);
                return output.Contains("Desbloqueado") || output.Contains("Unlocked");
            }
            catch
            {
                // Si no puede detectar, mejor no bloquear el flujo
                return false;
            }
        }

    }
}
