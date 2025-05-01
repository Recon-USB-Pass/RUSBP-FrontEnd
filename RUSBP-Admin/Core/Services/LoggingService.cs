using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Core/Services/LoggingService.cs
namespace RUSBP_Admin.Core.Services;
public static class LoggingService
{
    private const string LOG_DIR = "logs";
    public static void Debug(string msg)
    {
        Directory.CreateDirectory(LOG_DIR);
        File.AppendAllText(Path.Combine(LOG_DIR, "debug.txt"),
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {msg}{Environment.NewLine}");
    }
    public static void Evento(string serial, string evento)
    {
        if (string.IsNullOrWhiteSpace(serial)) return;
        Directory.CreateDirectory(LOG_DIR);
        var shortId = serial.Length > 8 ? serial[^8..] : serial;
        File.AppendAllText(Path.Combine(LOG_DIR, $"usb_{shortId}.txt"),
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {evento}{Environment.NewLine}");
    }
}

