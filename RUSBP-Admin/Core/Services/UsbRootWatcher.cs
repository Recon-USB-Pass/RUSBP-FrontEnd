//namespace RUSBP_Admin.Core.Services

using System;
using System.IO;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Services
{
    public class UsbRootWatcher : IDisposable
    {
        public event Action<string>? RootDriveDetected; // Ej: "E:"

        private readonly ManagementEventWatcher _insertWatcher;
        private readonly ManagementEventWatcher _removeWatcher;

        public UsbRootWatcher()
        {
            _insertWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2")); // insert
            _removeWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3")); // remove

            _insertWatcher.EventArrived += (_, __) => Task.Run(CheckForRootUsb);
            _removeWatcher.EventArrived += (_, __) => Task.Run(CheckForRootUsb);

            _insertWatcher.Start();
            _removeWatcher.Start();

            Task.Run(CheckForRootUsb); // Chequeo inicial
        }

        private void CheckForRootUsb()
        {
            // Espera pequeño para que Windows monte la unidad
            Thread.Sleep(800);

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    string sysDir = Path.Combine(drive.RootDirectory.FullName, "rusbp.sys");
                    string pkiDir = Path.Combine(drive.RootDirectory.FullName, "pki");
                    if (Directory.Exists(sysDir) && Directory.Exists(pkiDir))
                    {
                        // Se encontró una unidad candidata válida
                        RootDriveDetected?.Invoke(drive.Name.Substring(0, 2)); // "E:"
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _insertWatcher.Stop();
                _removeWatcher.Stop();
                _insertWatcher.Dispose();
                _removeWatcher.Dispose();
            }
            catch { }
        }
    }
}


