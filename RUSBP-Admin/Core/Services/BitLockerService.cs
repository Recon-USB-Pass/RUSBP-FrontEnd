using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Core/Services/BitLockerService.cs
namespace RUSBP_Admin.Core.Services;
public static class BitLockerService
{
    public static void TryUnlock(string letter, string password)
    {
        try
        {
            var psi = new ProcessStartInfo("cmd.exe",
                $"/c manage-bde -unlock {letter.TrimEnd('\\')} -password {password} >nul 2>&1")
            { UseShellExecute = false, CreateNoWindow = true };
            using var p = Process.Start(psi);
            p?.WaitForExit(8000);
            LoggingService.Debug($"Unlock {letter} exit={p?.ExitCode}");
        }
        catch (Exception ex) { LoggingService.Debug($"Unlock error {ex.Message}"); }
    }
    public static bool IsLocked(string driveLetter)
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

            // Ejemplo de línea relevante:
            // "    Estado de bloqueo:      Bloqueado"
            var match = Regex.Match(output, @"Estado de bloqueo:\s+(Bloqueado|Desbloqueado)", RegexOptions.IgnoreCase);
            if (!match.Success) return false;                 // <-- no encontrado ⇒ asumimos desbloqueado

            return match.Groups[1].Value.StartsWith("Bloqueado", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            // Cualquier error ⇒ asumimos desbloqueado y NO lanzamos UI
            return false;
        }
    }
}
