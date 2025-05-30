using RUSBP_Admin.Core.Helpers;
using RUSBP_Admin.Core.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms.Vistas
{
    // Para evitar ambigüedad entre los tipos Button/ProgressBar de Forms y VisualStyles:
    using ProgressBar = System.Windows.Forms.ProgressBar;
    using Button = System.Windows.Forms.Button;

    public partial class UsbAssignmentView : UserControl
    {
        private readonly ApiClient _api;
        private readonly UsbCryptoService _usb;
        private DriveInfo? _currentUsb;
        private string? _serial;
        private string? _recoveryPassX;
        private bool _usbEncrypted = false;

        public event Action<string>? UsbPrepared;

        public UsbAssignmentView(ApiClient api, UsbCryptoService usb)
        {
            _api = api;
            _usb = usb;
            InitializeComponent();
            ToggleForm(false);
        }

        // 1. Detección de USB
        public class UsbCandidate
        {
            public string DriveLetter { get; set; } = "";
            public string Label { get; set; } = "";
            public long Size { get; set; }
            public string Serial { get; set; } = "";
            public override string ToString() => $"{Label} ({DriveLetter})";
        }

        private static UsbCandidate[] GetUnassignedUsbDrives()
        {
            return DriveInfo.GetDrives()
                .Where(d =>
                    (d.DriveType is DriveType.Removable or DriveType.Fixed) &&
                    d.IsReady &&
                    !d.Name.Equals("C:\\", StringComparison.OrdinalIgnoreCase))
                .Select(d => new
                {
                    Drive = d,
                    HasPki = Directory.Exists(Path.Combine(d.RootDirectory.FullName, "pki")),
                    HasCfg = File.Exists(Path.Combine(d.RootDirectory.FullName, "config.json"))
                })
                .Where(x => !x.HasPki && !x.HasCfg)
                .Select(x => new UsbCandidate
                {
                    DriveLetter = x.Drive.RootDirectory.FullName,
                    Label = x.Drive.VolumeLabel,
                    Size = x.Drive.TotalSize,
                    Serial = GetUsbSerial(x.Drive.RootDirectory.FullName)
                })
                .ToArray();
        }

        private void btnDetect_Click(object? s, EventArgs e)
        {
            var candidatos = GetUnassignedUsbDrives();
            if (candidatos.Length == 0)
            {
                Log("⚠️  No hay unidades inicializables (sin archivos PKI)");
                return;
            }

            UsbCandidate elegido = candidatos.Length == 1 ? candidatos[0] : SelectUsbDialog(candidatos);
            if (elegido == null) return;

            _currentUsb = new DriveInfo(elegido.DriveLetter);
            _serial = elegido.Serial;
            _recoveryPassX = null;
            _usbEncrypted = false;
            _lblUsbName.Text = elegido.Label;
            _lblUsbSize.Text = $"{elegido.Size / 1_073_741_824} GB";
            Log($"Disco listo para asignar: {_currentUsb.Name}  Serial={_serial}");

            ToggleForm(true);
            progressBar.Value = 0;
            progressBar.Refresh();
        }

        // 2. Botón "Cifrar Unidad"
        private async void btnEncrypt_Click(object? s, EventArgs e)
        {
            if (_currentUsb is null)
            {
                MessageBox.Show("Selecciona un USB primero.");
                return;
            }

            Log("Cifrando unidad... NO retire el USB.");
            try
            {
                _recoveryPassX = await DriveEncryption.EncryptDriveWithBitLockerAsync(
                    _currentUsb.RootDirectory.FullName,
                    percent => Invoke((MethodInvoker)(() =>
                    {
                        progressBar.Value = percent;
                        progressBar.Refresh();
                        Log($"Progreso cifrado: {percent}%");
                    }))
                );
                _usbEncrypted = true;
                Log("Unidad cifrada correctamente. RecoveryPassword obtenida.");
                EnableUserInputs(true);
            }
            catch (Exception ex)
            {
                Log($"Error en cifrado: {ex.Message}");
                _usbEncrypted = false;
            }
        }

        // 3. Generar PKI
        private void btnGenPki_Click(object? s, EventArgs e)
        {
            if (_currentUsb is null || string.IsNullOrEmpty(_serial) || _serial == "UNKNOWN" || !_usbEncrypted)
            {
                Log("Selecciona un USB cifrado primero.");
                return;
            }

            try
            {
                var destPki = Path.Combine(_currentUsb.RootDirectory.FullName, "pki");
                Directory.CreateDirectory(destPki);
                var (certPath, privPath) = PkiService.GeneratePkcs8KeyPair(_serial, destPki);
                bool ok = File.Exists(certPath) && File.Exists(privPath);
                Log(ok ? "Claves PKI generadas en el USB." : "Error generando PKI.");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        // 4. Crear usuario y registrar USB
        private async void btnCreate_Click(object? s, EventArgs e)
        {
            if (_currentUsb is null || !_usbEncrypted || string.IsNullOrWhiteSpace(_recoveryPassX))
            {
                MessageBox.Show("Debes cifrar la unidad primero.");
                return;
            }
            if (_serial is null || _serial == "UNKNOWN")
            {
                MessageBox.Show("Presiona «Detectar USB» – no se obtuvo el serial.");
                return;
            }
            var pkiDir = Path.Combine(_currentUsb.RootDirectory.FullName, "pki");
            if (!File.Exists(Path.Combine(pkiDir, "cert.crt")) || !File.Exists(Path.Combine(pkiDir, "priv.key")))
            {
                Directory.CreateDirectory(pkiDir);
                var (certPath, privPath) = PkiService.GeneratePkcs8KeyPair(_serial, pkiDir);
                if (!File.Exists(certPath) || !File.Exists(privPath))
                {
                    MessageBox.Show("Error generando PKI en el USB.");
                    return;
                }
            }
            // Thumbprint
            string certFile = Path.Combine(pkiDir, "cert.crt");
            string thumbprint = "";
            try
            {
                var certBytes = File.ReadAllBytes(certFile);
                string certText = File.ReadAllText(certFile);
                if (certText.Contains("BEGIN CERTIFICATE"))
                {
                    string base64 = string.Join("", certText
                        .Split('\n')
                        .Where(line => !line.StartsWith("-----")));
                    certBytes = Convert.FromBase64String(base64.Trim());
                }
                var x509 = new X509Certificate2(certBytes);
                thumbprint = x509.Thumbprint ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error leyendo cert.crt o calculando thumbprint: {ex.Message}");
                return;
            }
            // Crear usuario
            var dtoUser = new
            {
                rut = txtRut.Text.Trim(),
                nombre = txtNombre.Text.Trim(),
                depto = txtDepto.Text.Trim(),
                email = txtMail.Text.Trim(),
                rol = cmbRol.SelectedItem?.ToString() ?? "Empleado",
                pin = txtPin.Text.Trim()
            };
            try
            {
                var resp = await _api.PostAsync<UsuarioCreatedResponse>("/api/usuarios", dtoUser);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API /api/usuarios: {ex.Message}");
                return;
            }
            // Alta USB
            var altaDto = new { serial = _serial, thumbprint = thumbprint };
            try
            {
                await _api.PostAsync("/api/usb", altaDto);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                // USB ya registrado (no es error)
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API /api/usb: {ex.Message}");
                return;
            }
            // Vincular USB ⇄ Usuario
            var vincDto = new { Serial = _serial, UsuarioRut = dtoUser.rut };
            try
            {
                await _api.PostAsync("/api/usb/asignar", vincDto);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API /api/usb/asignar: {ex.Message}");
                return;
            }
            // Cifrar RP_x con RP_root y publicar a backend
            // Debes reemplazar esto por SettingsStore.Load().rpRoot si no existe GetRpRoot()
            var settings = SettingsStore.Load();
            if (settings is null || string.IsNullOrEmpty(settings.Value.rpRoot))
            {
                MessageBox.Show("No se pudo recuperar la clave RP_root del administrador.");
                return;
            }
            string rpRoot = settings.Value.rpRoot;
            var key = CryptoHelper.DeriveKeyFromPass(rpRoot);
            var (cipher, tag) = CryptoHelper.EncryptAesGcm(_recoveryPassX, key);
            var dtoRp = new
            {
                serial = _serial,
                rpCipher = Convert.ToBase64String(cipher),
                rpTag = Convert.ToBase64String(tag),
                adminRut = /* RUT admin logueado */ "ADMIN_RUT"
            };
            try
            {
                await _api.PostAsync("/api/usb/register", dtoRp);
                Log("RP_x cifrada y registrada en el backend.");
            }
            catch (Exception ex)
            {
                Log($"Error registrando RP_x en backend: {ex.Message}");
            }
            // Copiar config.json
            var cfg = new
            {
                dtoUser.nombre,
                dtoUser.rut,
                dtoUser.email,
                dtoUser.rol,
                Serial = _serial,
                Fecha = DateTime.UtcNow
            };
            File.WriteAllText(
                Path.Combine(_currentUsb.RootDirectory.FullName, "config.json"),
                JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true })
            );
            MessageBox.Show("Empleado + USB registrados ✔️");
            UsbPrepared?.Invoke(_serial);
        }

        private static UsbCandidate SelectUsbDialog(UsbCandidate[] opciones)
        {
            using var frm = new Form
            {
                Text = "Seleccione unidad para asignar",
                Size = new System.Drawing.Size(520, 240),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };
            var lst = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Segoe UI", 11),
                SelectionMode = SelectionMode.One
            };
            lst.Items.AddRange(opciones.Select(c =>
                $"{c.DriveLetter} – {c.Label}   {c.Size / 1_073_741_824} GB  (SN:{c.Serial})"
            ).ToArray());
            lst.SelectedIndex = 0;
            var ok = new Button { Text = "Aceptar", Dock = DockStyle.Bottom, Height = 38, DialogResult = DialogResult.OK };
            frm.Controls.Add(lst);
            frm.Controls.Add(ok);
            return frm.ShowDialog() == DialogResult.OK && lst.SelectedIndex >= 0
                ? opciones[lst.SelectedIndex]
                : null!;
        }

        // Helpers de serial
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetVolumeInformation(
            string lpRootPathName,
            System.Text.StringBuilder lpVolumeNameBuffer,
            uint nVolumeNameSize,
            out uint lpVolumeSerialNumber,
            out uint lpMaximumComponentLength,
            out uint lpFileSystemFlags,
            System.Text.StringBuilder lpFileSystemNameBuffer,
            uint nFileSystemNameSize);

        private static string GetUsbSerial(string driveLetter)
        {
            driveLetter = driveLetter.TrimEnd('\\', ':');
            try
            {
                string targetDiskDeviceId = "";
                using (var logicalDiskQuery = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}:'}} WHERE AssocClass=Win32_LogicalDiskToPartition"))
                {
                    foreach (ManagementObject partition in logicalDiskQuery.Get())
                    {
                        using (var diskQuery = new ManagementObjectSearcher(
                            $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition"))
                        {
                            foreach (ManagementObject disk in diskQuery.Get())
                            {
                                targetDiskDeviceId = disk["DeviceID"]?.ToString() ?? "";
                                break;
                            }
                        }
                    }
                }
                using (var drives = new ManagementObjectSearcher("SELECT DeviceID, SerialNumber, InterfaceType FROM Win32_DiskDrive WHERE InterfaceType='USB'"))
                {
                    foreach (ManagementObject drive in drives.Get())
                    {
                        var devId = drive["DeviceID"]?.ToString() ?? "";
                        var sn = (drive["SerialNumber"]?.ToString() ?? "").Trim();
                        if (devId == targetDiskDeviceId && !string.IsNullOrWhiteSpace(sn))
                        {
                            return sn.ToUpperInvariant();
                        }
                    }
                }
            }
            catch { }
            try
            {
                if (GetVolumeInformation($@"{driveLetter}:\", null, 0, out uint volSer, out _, out _, null, 0))
                {
                    var fallbackSerial = volSer.ToString("X8");
                    return fallbackSerial;
                }
            }
            catch { }
            return "UNKNOWN";
        }

        private void ToggleForm(bool enabled)
        {
            btnGenPki.Enabled = btnCreate.Enabled = btnEncrypt.Enabled = enabled;
        }

        private void EnableUserInputs(bool enabled)
        {
            btnGenPki.Enabled = btnCreate.Enabled = enabled;
        }

        private void Log(string msg)
        {
            logsTextBox.AppendText($"{DateTime.Now:HH:mm:ss}  {msg}{Environment.NewLine}");
        }

        private record UsuarioCreatedResponse(int id, string? msg);
    }
}
