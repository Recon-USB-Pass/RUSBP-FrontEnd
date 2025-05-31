/*


using System;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;

namespace RUSBP_Admin.Core.Helpers
{
    public class WaitForRootUsbForm : Form
    {
        private Label _lbl;

        public string? SelectedDriveRoot { get; private set; } = null;

        public WaitForRootUsbForm()
        {
            Width = 420;
            Height = 160;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _lbl = new Label()
            {
                Text = "Por favor conecte el USB root cifrado (con rusbp.sys y pki).",
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            Controls.Add(_lbl);

            Shown += (s, e) => StartWatcher();
        }

        private ManagementEventWatcher? watcher;

        private void StartWatcher()
        {
            // WMI: Evento de inserción de unidad lógica
            var q = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher = new ManagementEventWatcher(q);
            watcher.EventArrived += (s, e) =>
            {
                // Revisa si ahora hay alguna unidad válida (usb root)
                var usbList = Core.Services.UsbCryptoService.EnumerateUsbInfos();
                if (usbList.Count > 0)
                {
                    var first = usbList.First();
                    var root = first.Roots.FirstOrDefault();
                    if (root != null)
                    {
                        SelectedDriveRoot = root;
                        watcher.Stop();
                        BeginInvoke((Action)(() => DialogResult = DialogResult.OK));
                    }
                }
            };
            watcher.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            watcher?.Stop();
            watcher?.Dispose();
            base.OnFormClosed(e);
        }
    }
}


*/