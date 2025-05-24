using System;
using Microsoft.Win32;

namespace rusbp_bootstrap.Core
{
    public static class BootstrapHelpers
    {
        public static string GetWindowsEdition()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                var edition = key?.GetValue("EditionID")?.ToString();
                return edition ?? "UNKNOWN";
            }
        }

        public static string Prompt(string label, string hint, string def)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{label} ");
            Console.ResetColor();
            Console.Write($"[{hint}]  (Enter = \"{def}\"): ");
            string? v = Console.ReadLine();
            return string.IsNullOrWhiteSpace(v) ? def : v.Trim();
        }
    }
}
