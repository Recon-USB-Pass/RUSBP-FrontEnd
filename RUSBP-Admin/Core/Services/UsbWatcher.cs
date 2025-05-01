using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using RUSBP_Admin.Core.Services;   // LoggingService & BitLockerService

namespace RUSBP_Admin.Core.Services
{
    /// <summary>
    /// Vigila conexiones/desconexiones de volúmenes USB, intenta
    /// desbloquear BitLocker y levanta un evento indicando si la llave es válida
    /// (es decir, contiene <c>key.txt</c>).
    /// </summary>
    public sealed class UsbWatcher : IDisposable
    {
        private readonly ManagementEventWatcher _insertWatcher;
        private readonly ManagementEventWatcher _removeWatcher;
        private HashSet<string> _prevSerials = new();

        public event Action<bool>? StateChanged;  // true → USB válido presente

        private const string BITLOCKER_PASS = "Zarate_123";   // o léelo de ConfigService
        private const string LOG_DIR = "logs";

        public UsbWatcher()
        {
            _insertWatcher = new(new WqlEventQuery(
                "SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2"));
            _removeWatcher = new(new WqlEventQuery(
                "SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3"));

            EventArrivedEventHandler h = async (_, __) => await RefreshAsync();
            _insertWatcher.EventArrived += h;
            _removeWatcher.EventArrived += h;

            _insertWatcher.Start();
            _removeWatcher.Start();

            _ = RefreshAsync(); // chequeo inicial
        }

        /* --------------- núcleo --------------- */
        private async Task RefreshAsync()
        {
            await Task.Delay(500); // espera que Windows monte la letra

            var infos = EnumerarUsbInfos();
            var current = infos.Select(i => i.Serial).ToHashSet();

            foreach (var s in current.Except(_prevSerials))
                LogEvento(s, "Conectado");
            foreach (var s in _prevSerials.Except(current))
                LogEvento(s, "Desconectado");

            _prevSerials = current;

            bool llaveOk = false;

            foreach (var info in infos)
            {
                // intenta desbloquear todas las particiones por si están cifradas
                foreach (var letter in info.Letters)
                    BitLockerService.TryUnlock(letter, BITLOCKER_PASS);

                if (ExisteKeyTxt(info.Letters))
                {
                    LoggingService.Debug($"USB válido detectado: {info.Serial}");
                    llaveOk = true;
                    break;
                }
            }

            StateChanged?.Invoke(llaveOk);
        }

        /* -------- Enumeración de discos -------- */
        private record UsbInfo(string Serial, List<string> Letters);

        private static string SerialFromPnp(string pnp) =>
            pnp.Split('\\').LastOrDefault()?.Split('&').FirstOrDefault() ?? "";

        private static List<UsbInfo> EnumerarUsbInfos()
        {
            var list = new List<UsbInfo>();

            var ddr = new ManagementObjectSearcher(
                "SELECT DeviceID, PNPDeviceID FROM Win32_DiskDrive WHERE InterfaceType='USB'");

            foreach (ManagementObject d in ddr.Get())
            {
                string serial = SerialFromPnp(d["PNPDeviceID"]?.ToString() ?? "");
                if (serial == "") continue;

                var letters = new List<string>();
                foreach (ManagementObject part in d.GetRelated("Win32_DiskPartition"))
                    foreach (ManagementObject log in part.GetRelated("Win32_LogicalDisk"))
                        letters.Add(log["DeviceID"].ToString() + @"\");

                list.Add(new UsbInfo(serial, letters));
            }
            return list;
        }

        /* ------------- key.txt presente ------------- */
        private static bool ExisteKeyTxt(IEnumerable<string> letters)
        {
            foreach (var letter in letters)
            {
                string root = letter.EndsWith(@"\") ? letter : letter + @"\";
                try
                {
                    if (Directory.Exists(root))
                    {
                        if (File.Exists(Path.Combine(root, "key.txt")))
                            return true;
                    }
                }
                catch (UnauthorizedAccessException) { /* ignorar */ }
            }
            return false;
        }

        /* ---------------- LOG helpers ---------------- */
        private static void LogEvento(string serial, string evento)
        {
            if (string.IsNullOrWhiteSpace(serial)) return;

            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);

            string shortId = serial.Length > 8 ? serial[^8..] : serial;
            string path = Path.Combine(logDir, $"usb_{shortId}.txt");
            string ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            File.AppendAllText(path, $"{ts} - {evento}{Environment.NewLine}");
        }
        private static void LogDebug(string msg)
        {
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDir);
            string ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText(Path.Combine(logDir, "debug.txt"), $"{ts} - {msg}{Environment.NewLine}");
        }



        /* ---------------- limpieza ---------------- */
        public void Dispose()
        {
            _insertWatcher.Stop();
            _removeWatcher.Stop();
            _insertWatcher.Dispose();
            _removeWatcher.Dispose();
        }
    }
}
