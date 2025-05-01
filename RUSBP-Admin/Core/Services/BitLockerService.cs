using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
}
