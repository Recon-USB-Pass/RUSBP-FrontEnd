// UsbAssignmentView.cs  (lógica)
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Windows.Forms;
using BCrypt.Net;                       // BCrypt.Net-Next
using RUSBP_Admin.Core.Services;        // ApiClient  y  UsbCryptoService

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class UsbAssignmentView : UserControl
    {
        /* =========  DI  ========= */
        private readonly ApiClient _api;
        private readonly UsbCryptoService _usb;

        /* =========  estado  ===== */
        private DriveInfo? _currentUsb;
        private string? _serial;
        private string? _workDir;

        public event Action<string /*serial*/>? UsbPrepared;


        private Panel pnlCert = new Panel { Width = 32, Height = 32, BackColor = Color.Gray };
        private Panel pnlPriv = new Panel { Width = 32, Height = 32, BackColor = Color.Gray };

        /* =========  ctor  ======= */
        public UsbAssignmentView(ApiClient api, UsbCryptoService usb)
        {
            _api = api;
            _usb = usb;
            InitializeComponent();      // ← del archivo *.Designer.cs
            ToggleForm(false);          // deshabilita campos hasta detectar
        }

        /* ===============================================================
           1. Detectar USB
        ===============================================================*/

        public class UsbCandidate
        {
            public string DriveLetter { get; set; } = "";
            public string Label { get; set; } = "";
            public long Size { get; set; }
            public string Serial { get; set; } = "";
            public override string ToString() => $"{Label} ({DriveLetter})";
        }

        // Devuelve solo los USBs que NO tienen archivos de agente
        public static List<UsbCandidate> GetUnassignedUsbDrives()
        {
            var result = new List<UsbCandidate>();
            foreach (var drive in DriveInfo.GetDrives().Where(d =>
                (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed) &&
                d.IsReady &&
                !d.Name.Equals("C:\\", StringComparison.OrdinalIgnoreCase)))
            {
                var root = drive.RootDirectory.FullName;
                // Chequear si existen PKI dentro de la subcarpeta pki
                var pkiDir = Path.Combine(root, "pki");
                bool hasPem = false;
                if (Directory.Exists(pkiDir))
                {
                    hasPem =
                        Directory.GetFiles(pkiDir, "*.pem", SearchOption.TopDirectoryOnly).Any() ||
                        Directory.GetFiles(pkiDir, "*.crt", SearchOption.TopDirectoryOnly).Any() ||
                        Directory.GetFiles(pkiDir, "*.key", SearchOption.TopDirectoryOnly).Any();
                }
                // Además, verifica si existe el config.json (en raíz, como lo generas)
                bool hasConfig = File.Exists(Path.Combine(root, "config.json"));

                if (!hasPem && !hasConfig)
                {
                    result.Add(new UsbCandidate
                    {
                        DriveLetter = root,
                        Label = drive.VolumeLabel,
                        Size = drive.TotalSize,
                        Serial = GetVolumeSerial(root.TrimEnd('\\'))
                    });
                }
            }
            return result;
        }

        private bool UsbTienePKI(DriveInfo drive)
        {
            var pkiDir = Path.Combine(drive.RootDirectory.FullName, "pki");
            return Directory.Exists(pkiDir) &&
                   (Directory.GetFiles(pkiDir, "*.pem", SearchOption.TopDirectoryOnly).Any() ||
                    Directory.GetFiles(pkiDir, "*.crt", SearchOption.TopDirectoryOnly).Any() ||
                    Directory.GetFiles(pkiDir, "*.key", SearchOption.TopDirectoryOnly).Any());
        }

        private void SetSelectedUsb(UsbCandidate usb)
        {
            _currentUsb = new DriveInfo(usb.DriveLetter);
            _serial = usb.Serial;
            _lblUsbName.Text = usb.Label;
            _lblUsbSize.Text = $"{usb.Size / 1_073_741_824} GB";
            Log($"USB seleccionado: {_currentUsb.Name}  Serial={_serial}");
            ToggleForm(true);
        }
        private void btnDetect_Click(object? s, EventArgs e)
        {
            // Lista de discos Removable o Fixed, excepto C:, y sin archivos .pem
            var discos = DriveInfo.GetDrives()
                .Where(d =>
                    (d.DriveType == DriveType.Removable || d.DriveType == DriveType.Fixed) &&
                    d.IsReady &&
                    !Directory.GetFiles(d.RootDirectory.FullName, "*.pem", SearchOption.TopDirectoryOnly).Any() &&
                    !d.Name.Equals("C:\\", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!discos.Any())
            {
                Log("⚠️  No hay unidades inicializables (sin archivos PKI)");
                return;
            }

            // Si hay más de uno, mostrar un dialog para elegir
            DriveInfo? seleccionado = null;
            if (discos.Count == 1)
            {
                seleccionado = discos[0];
            }
            else
            {
                // Construir un string con los nombres y tamaños
                var opciones = discos.Select(d =>
                    $"{d.Name}   ({(d.DriveType == DriveType.Removable ? "USB" : "Disco Fijo")}, {d.VolumeLabel}, {d.TotalSize / 1_073_741_824} GB)"
                ).ToArray();

                // Mostrar un diálogo de selección simple
                var frm = new Form
                {
                    Text = "Seleccione unidad para asignar",
                    Size = new Size(500, 200),
                    StartPosition = FormStartPosition.CenterParent
                };

                var list = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 12)
                };
                list.Items.AddRange(opciones);

                list.SelectionMode = SelectionMode.One;
                list.SelectedIndex = 0;

                var btnOk = new Button
                {
                    Text = "OK",
                    Dock = DockStyle.Bottom,
                    Height = 40,
                    DialogResult = DialogResult.OK
                };

                frm.Controls.Add(list);
                frm.Controls.Add(btnOk);

                if (frm.ShowDialog() == DialogResult.OK && list.SelectedIndex >= 0)
                {
                    seleccionado = discos[list.SelectedIndex];
                }
            }

            if (seleccionado == null) return;

            _currentUsb = seleccionado;
            _serial = GetVolumeSerial(_currentUsb.RootDirectory.FullName.TrimEnd('\\'));
            _lblUsbName.Text = _currentUsb.VolumeLabel;
            _lblUsbSize.Text = $"{_currentUsb.TotalSize / 1_073_741_824} GB";
            Log($"Disco listo para asignar: {_currentUsb.Name}  Serial={_serial}");
            ToggleForm(true);
        }





        private static string GetVolumeSerial(string driveLetter)
        {
            try
            {
                using var mo = new ManagementObject(
                    $@"Win32_LogicalDisk.DeviceID=""{driveLetter}:""");
                mo.Get();
                return mo["VolumeSerialNumber"]?.ToString() ?? "UNKNOWN";
            }
            catch { return "UNKNOWN"; }
        }

        /* ===============================================================
           2. Generar PKI
        ===============================================================*/
        private void btnGenPki_Click(object? s, EventArgs e)
        {
            if (_currentUsb is null || string.IsNullOrEmpty(_serial))
            {
                Log("Primero seleccione un USB válido.");
                return;
            }

            try
            {
                // Crear la carpeta 'pki' si no existe
                var destPki = Path.Combine(_currentUsb.RootDirectory.FullName, "pki");
                Directory.CreateDirectory(destPki); // <-- Esta línea es clave

                var (certPath, privPath) = PkiService.GeneratePkcs8KeyPair(_serial, destPki);

                bool ok = File.Exists(certPath) && File.Exists(privPath);
                pnlCert.BackColor = ok ? Color.LimeGreen : Color.Red;
                pnlPriv.BackColor = ok ? Color.LimeGreen : Color.Red;

                Log(ok ? "Claves PKI generadas en el USB." : "Error generando PKI.");
            }
            catch (Exception ex)
            {
                pnlCert.BackColor = Color.Red;
                pnlPriv.BackColor = Color.Red;
                Log($"Error: {ex.Message}");
            }
        }



        /* ===============================================================
           3. Guardar empleado + asignar USB
        ===============================================================*/
        private async void btnCreate_Click(object? s, EventArgs e)
        {
            if (_currentUsb is null)
            {
                MessageBox.Show("Detecta un USB válido primero.");
                return;
            }
            var pkiPath = Path.Combine(_currentUsb.RootDirectory.FullName, "pki");
            if (!Directory.Exists(pkiPath) ||
                !File.Exists(Path.Combine(pkiPath, "cert.crt")) ||
                !File.Exists(Path.Combine(pkiPath, "priv.key")))
            {
                MessageBox.Show("Debes generar PKI primero.");
                return;
            }

            /* 3 .1  crea usuario en backend */
            var dtoUser = new
            {
                nombre = txtNombre.Text.Trim(),
                rut = txtRut.Text.Trim(),
                depto = txtDepto.Text.Trim(),
                email = txtMail.Text.Trim(),
                rol = cmbRol.SelectedItem!.ToString(),
                pinHash = BCrypt.Net.BCrypt.HashPassword(txtPin.Text.Trim())
            };
            int userId;
            try
            {
                userId = await _api.PostAsync<int>("/usuarios", dtoUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error API /usuarios: {ex.Message}");
                return;
            }

            /* 3 .2  asigna USB en backend */
            var dtoUsb = new { Serial = _serial, UsuarioId = userId };
            try
            {
                await _api.PostAsync("/usb/asignar", dtoUsb);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error API /usb/asignar: {ex.Message}");
                return;
            }

            /* 3 .3  copiar PKI + config al pendrive */
            CopyDir(_workDir, Path.Combine(_currentUsb.RootDirectory.FullName, "pki"));

            var cfg = new
            {
                dtoUser.nombre,
                dtoUser.rut,
                dtoUser.email,
                dtoUser.rol,
                Serial = _serial,
                Fecha = DateTime.UtcNow
            };
            File.WriteAllText(Path.Combine(_currentUsb.RootDirectory.FullName, "config.json"),JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));

            Log("Empleado + USB registrados ✔️");
        }

        /* ===============================================================
           4. helpers
        ===============================================================*/
        private static void CopyDir(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var f in Directory.GetFiles(src))
                File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), true);

            foreach (var d in Directory.GetDirectories(src))
                CopyDir(d, Path.Combine(dst, Path.GetFileName(d)));
        }

        private void ToggleForm(bool enabled)
        {
            btnGenPki.Enabled =
            btnCreate.Enabled = enabled;
        }

        private void Log(string msg) =>
            logsTextBox.AppendText($"{DateTime.Now:HH:mm:ss}  {msg}{Environment.NewLine}");

    }
}
