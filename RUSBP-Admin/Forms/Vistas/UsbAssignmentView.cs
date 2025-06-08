// RUSBP_Admin.Forms.Vistas.UsbAssignmentView.cs
// ------------------------------------------------------------
// Vista de asignación de USB · cifrado + PKI + registro backend
// ------------------------------------------------------------

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
        private string? _lastCreatedUserRut;

        public event Action<string>? UsbPrepared;   // callback al terminar OK

        // ──────────────────────────────────────────────────────────────
        public UsbAssignmentView(ApiClient api, UsbCryptoService usb)
        {
            _api = api;
            _usb = usb;
            InitializeComponent();
            ToggleForm(false);
        }

        // ═══════════════ 1. DETECCIÓN DE DISCO ═══════════════
        private sealed class UsbCandidate
        {
            public string DriveLetter { get; init; } = "";
            public string Label { get; init; } = "";
            public long Size { get; init; }
            public string Serial { get; init; } = "";
            public override string ToString() => $"{Label} ({DriveLetter})";
        }

        private static UsbCandidate[] GetUnassignedUsbDrives() =>
            DriveInfo.GetDrives()
                     .Where(d => (d.DriveType is DriveType.Removable or DriveType.Fixed) &&
                                 d.IsReady &&
                                 !d.Name.Equals("C:\\", StringComparison.OrdinalIgnoreCase))
                     .Select(d => new
                     {
                         Drive = d,
                         HasPki = Directory.Exists(Path.Combine(d.RootDirectory.FullName, "pki")),
                         HasCfg = File.Exists(Path.Combine(d.RootDirectory.FullName, "config.json"))
                     })
                     // Sólo discos sin entorno previo
                     .Where(x => !x.HasPki && !x.HasCfg)
                     .Select(x => new UsbCandidate
                     {
                         DriveLetter = x.Drive.RootDirectory.FullName,
                         Label = x.Drive.VolumeLabel,
                         Size = x.Drive.TotalSize,
                         Serial = GetUsbSerial(x.Drive.RootDirectory.FullName)
                     })
                     .ToArray();

        private void btnDetect_Click(object? s, EventArgs e)
        {
            var discos = GetUnassignedUsbDrives();
            if (discos.Length == 0)
            {
                Log("⚠️  No hay unidades inicializables (sin archivos PKI/config).");
                return;
            }

            var elegido = discos.Length == 1 ? discos[0] : SelectUsbDialog(discos);
            if (elegido is null) return;

            _currentUsb = new DriveInfo(elegido.DriveLetter);
            _serial = elegido.Serial;
            _recoveryPassX = null;
            _usbEncrypted = false;

            _lblUsbName.Text = string.IsNullOrWhiteSpace(elegido.Label) ? "(Sin etiqueta)" : elegido.Label;
            _lblUsbSize.Text = $"{elegido.Size / 1_073_741_824} GB";
            Log($"Disco listo para asignar: {_currentUsb.Name}  Serial={_serial}");

            ToggleForm(true);
            progressBar.Value = 0; progressBar.Refresh();

            // Reset indicadores
            chkCert.Checked = chkPriv.Checked = chkCfg.Checked = false;
        }

        // ═══════════════ 2. CIFRAR UNIDAD (BitLocker) ═══════════════
        private async void btnEncrypt_Click(object? s, EventArgs e)
        {
            if (_currentUsb is null)
            {
                MessageBox.Show("Selecciona un USB primero.");
                return;
            }

            Log("Cifrando unidad… NO retire el USB.");
            try
            {
                _recoveryPassX = await DriveEncryption.EncryptDriveWithBitLockerAsync(
                    _currentUsb.RootDirectory.FullName,
                    percent => Invoke((MethodInvoker)(() =>
                    {
                        progressBar.Value = percent;
                        Log($"Progreso cifrado: {percent}%");
                    }))
                );

                _usbEncrypted = true;
                Log("Unidad cifrada correctamente. RecoveryPassword (RP_x) obtenida.");
                Log($"RP_x generada: {_recoveryPassX}");

                // Si quedó desbloqueada ⇒ se bloquea para evitar escritura accidental
                if (!BitLockerService.IsLocked(_currentUsb.RootDirectory.FullName))
                    CryptoHelper.LockDrive(_currentUsb.RootDirectory.FullName);

                EnableUserInputs(true);
                chkPriv.Checked = true;   // marco «clave privada cifrada» (se generará luego)
            }
            catch (Exception ex)
            {
                _usbEncrypted = false;
                Log($"❌ Error en cifrado: {ex.Message}");
            }
        }

        // ═══════════════ 3. GENERAR PKI ═══════════════
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

                chkCert.Checked = File.Exists(certPath);
                chkPriv.Checked = File.Exists(privPath);

                Log(chkCert.Checked && chkPriv.Checked
                        ? "✅ Claves PKI generadas en el USB."
                        : "❌ Error generando PKI.");
            }
            catch (Exception ex) { Log($"❌ Error PKI: {ex.Message}"); }
        }

        // ═══════════════ 4. CREAR USUARIO + REGISTRAR USB ═══════════════
        /// <summary>
        ///  Crea usuario, registra USB y envía RP_x al backend – con rollback seguro.
        ///  * Cumple con los DTO exactos publicados en Swagger.
        /// </summary>
        // ═══════════════ 4. CREAR USUARIO + REGISTRAR USB ═══════════════
        // ═══════════════ 4. CREAR USUARIO + REGISTRAR USB ═══════════════
        private async void btnCreate_Click(object? s, EventArgs e)
        {
            /* …validaciones previas aquí (sin cambios)… */

            var pkiDir = Path.Combine(_currentUsb!.RootDirectory.FullName, "pki");

            try
            {
                /* 1) Asegura PKI (sin cambios) */
                string certPath = Path.Combine(pkiDir, "cert.crt");
                string privPath = Path.Combine(pkiDir, "priv.key");
                if (!File.Exists(certPath) || !File.Exists(privPath))
                {
                    Directory.CreateDirectory(pkiDir);
                    (certPath, privPath) = PkiService.GeneratePkcs8KeyPair(_serial!, pkiDir);
                    if (!File.Exists(certPath) || !File.Exists(privPath))
                        throw new Exception("Error generando PKI en el USB.");
                }
                chkCert.Checked = chkPriv.Checked = true;

                // ──────── 2) Thumbprint  ────────
                string thumbprint = PkiService.GetThumbprintFromPem(certPath);


                /* 3) Crear usuario  (igual) */
                var dtoUser = new
                {
                    rut = txtRut.Text.Trim(),
                    nombre = txtNombre.Text.Trim(),
                    depto = txtDepto.Text.Trim(),
                    email = txtMail.Text.Trim(),
                    rol = cmbRol.SelectedItem!.ToString(),
                    pin = txtPin.Text.Trim()
                };
                await _api.PostAsync<UsuarioCreatedResponse>("/api/usuarios", dtoUser);
                _lastCreatedUserRut = dtoUser.rut;

                /* 4) Registrar USB (serial + thumbprint) */
                var dtoAlta = new { serial = _serial, thumbprint };
                try { await _api.PostAsync("/api/usb", dtoAlta); }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict) { }

                /* 5) RP_x cifrada → backend (igual) */
                var settings = SettingsStore.Load();
                if (settings is null || string.IsNullOrWhiteSpace(settings.Value.rpRoot))
                    throw new Exception("No se pudo recuperar la clave RP_root.");
                var key = CryptoHelper.DeriveKeyFromPass(settings.Value.rpRoot);
                var (ciph, tag) = CryptoHelper.EncryptAesGcm(_recoveryPassX!, key);
                int rolValor = dtoUser.rol.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                var dtoRp = new
                {
                    serial = _serial,
                    cipher = Convert.ToBase64String(ciph),
                    tag = Convert.ToBase64String(tag),
                    rol = rolValor
                };
                await _api.PostAsync("/api/usb/register", dtoRp);
                Log("RP_x cifrada y registrada en backend.");
                chkCfg.Checked = true;

                /* 6) Vincular USB ↔ Usuario (igual) */
                var dtoVinc = new { serial = _serial, usuarioRut = dtoUser.rut };
                await _api.PostAsync("/api/usb/asignar", dtoVinc);

                /* 7) Guardar config.json (igual) */
                var cfg = new
                {
                    dtoUser.nombre,
                    dtoUser.rut,
                    dtoUser.email,
                    dtoUser.rol,
                    Serial = _serial,
                    Fecha = DateTime.UtcNow
                };
                File.WriteAllText(Path.Combine(_currentUsb.RootDirectory.FullName, "config.json"),
                                  JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
                chkCfg.Checked = true;

                MessageBox.Show("Empleado + USB registrados ✔️");
                UsbPrepared?.Invoke(_serial!);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API: {ex.Message}");
                await RollbackBackendAndLocal();
            }
            catch (Exception ex)
            {
                Log($"❌ Error general: {ex.Message}");
                await RollbackBackendAndLocal();
            }
        }






        // ═══════════════ 5. ROLLBACK ═══════════════
        private async System.Threading.Tasks.Task RollbackBackendAndLocal()
        {
            try
            {
                if (_lastCreatedUserRut != null)
                    await _api.SendAsync(HttpMethod.Delete, $"/api/usuarios/{_lastCreatedUserRut}");
            }
            catch { /* ignora */ }

            RollbackLocalFiles();
            Log("Rollback realizado por error en la asignación.");
        }

        private void RollbackLocalFiles()
        {
            try
            {
                if (_currentUsb is null) return;
                var root = _currentUsb.RootDirectory.FullName;
                var pki = Path.Combine(root, "pki");
                var cfg = Path.Combine(root, "config.json");
                if (Directory.Exists(pki)) Directory.Delete(pki, true);
                if (File.Exists(cfg)) File.Delete(cfg);
                chkCert.Checked = chkPriv.Checked = chkCfg.Checked = false;
            }
            catch { }
        }

        // ═══════════════ Helpers UI & utilidades ═══════════════
        private static UsbCandidate? SelectUsbDialog(UsbCandidate[] opciones)
        {
            using var frm = new Form
            {
                Text = "Seleccione unidad para asignar",
                Size = new System.Drawing.Size(540, 250),
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
                $"{c.DriveLetter} – {c.Label}   {c.Size / 1_073_741_824} GB  (SN:{c.Serial})").ToArray());
            lst.SelectedIndex = 0;
            frm.Controls.Add(lst);
            frm.Controls.Add(new Button
            {
                Text = "Aceptar",
                Dock = DockStyle.Bottom,
                Height = 38,
                DialogResult = DialogResult.OK
            });

            return frm.ShowDialog() == DialogResult.OK && lst.SelectedIndex >= 0
                        ? opciones[lst.SelectedIndex]
                        : null;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetVolumeInformation(
            string lpRootPathName,
            System.Text.StringBuilder lpVolumeNameBuffer, uint nVolumeNameSize,
            out uint lpVolumeSerialNumber, out uint lpMaximumComponentLength,
            out uint lpFileSystemFlags,
            System.Text.StringBuilder lpFileSystemNameBuffer, uint nFileSystemNameSize);

        private static string GetUsbSerial(string driveLetter)
        {
            driveLetter = driveLetter.TrimEnd('\\', ':');

            try
            {
                // WMI — obtenemos el DeviceID del disco correspondiente
                string targetDeviceId = "";
                using var partQry = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}:'}} WHERE AssocClass=Win32_LogicalDiskToPartition");
                foreach (ManagementObject part in partQry.Get())
                {
                    using var diskQry = new ManagementObjectSearcher(
                        $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{part["DeviceID"]}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition");
                    foreach (ManagementObject disk in diskQry.Get())
                    {
                        targetDeviceId = disk["DeviceID"]?.ToString() ?? "";
                        break;
                    }
                }

                using var usbDisks = new ManagementObjectSearcher(
                    "SELECT DeviceID, SerialNumber FROM Win32_DiskDrive WHERE InterfaceType='USB'");
                foreach (ManagementObject d in usbDisks.Get())
                {
                    var devId = d["DeviceID"]?.ToString() ?? "";
                    var sn = d["SerialNumber"]?.ToString()?.Trim() ?? "";
                    if (devId == targetDeviceId && sn.Length > 0) return sn.ToUpperInvariant();
                }
            }
            catch { /* sigue con fallback */ }

            // Fallback: volumen serial (no único pero sirve)
            try
            {
                if (GetVolumeInformation($@"{driveLetter}:\", null, 0,
                                         out uint volSer, out _, out _, null, 0))
                    return volSer.ToString("X8");
            }
            catch { }

            return "UNKNOWN";
        }

        private void ToggleForm(bool enabled) => btnEncrypt.Enabled = enabled;
        private void EnableUserInputs(bool enabled) =>
            btnGenPki.Enabled = btnCreate.Enabled = enabled;

        private void Log(string msg) =>
            logsTextBox.AppendText($"{DateTime.Now:HH:mm:ss}  {msg}{Environment.NewLine}");

        private record UsuarioCreatedResponse(int id, string? msg);
    }
}
