// UsbAssignmentView.cs  (lógica)
// ─────────────────────────────────────────────────────────────────────────────
// Adaptado: 2025-05-22
//  • Se sustituye la obtención del serial usando WMI por llamada a Win32 API
//    GetVolumeInformation (kernel32), mucho más confiable en USBs protegidos.
//  • Se centraliza la lectura del serial en GetUsbSerial() y _serial sólo se
//    establece desde ese helper.
//  • Se comenta cualquier clase / método que ya no se usa y se explica “por qué”.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Forms;
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

        public event Action<string /*serial*/>? UsbPrepared;

        /* =========  ctor  ======= */
        public UsbAssignmentView(ApiClient api, UsbCryptoService usb)
        {
            _api = api;
            _usb = usb;
            InitializeComponent();
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

        /// <summary>
        /// Lista los discos inicializables (removable/fixed ≠ C:)
        /// y que NO contengan ya PKI/config.
        /// </summary>
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

            UsbCandidate elegido = candidatos.Length == 1
                ? candidatos[0]
                : SelectUsbDialog(candidatos);

            if (elegido == null) return;

            _currentUsb = new DriveInfo(elegido.DriveLetter);
            _serial = elegido.Serial;

            _lblUsbName.Text = elegido.Label;
            _lblUsbSize.Text = $"{elegido.Size / 1_073_741_824} GB";
            Log($"Disco listo para asignar: {_currentUsb.Name}  Serial={_serial}");

            ToggleForm(true);
        }

        #region Helpers de Serial (Win32 API)
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

        private static string GetUsbSerial(string root)
        {
            try
            {
                if (!root.EndsWith("\\"))
                    root += "\\";

                bool ok = GetVolumeInformation(
                    root,
                    null!,
                    0,
                    out uint serial,
                    out _,
                    out _,
                    null!,
                    0);

                return ok ? serial.ToString("X") : "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }
        #endregion

        #region Selección visual cuando hay varios USB
        private static UsbCandidate SelectUsbDialog(UsbCandidate[] opciones)
        {
            using var frm = new Form
            {
                Text = "Seleccione unidad para asignar",
                Size = new Size(520, 240),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedToolWindow
            };

            var lst = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11),
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
        #endregion

        /* ===============================================================
           2. Generar PKI
        ===============================================================*/
        private void btnGenPki_Click(object? s, EventArgs e)
        {
            if (_currentUsb is null || string.IsNullOrEmpty(_serial) || _serial == "UNKNOWN")
            {
                Log("Primero seleccione un USB válido.");
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
            if (_serial is null || _serial == "UNKNOWN")
            {
                MessageBox.Show("Primero presiona «Detectar USB» – no se obtuvo el serial.");
                return;
            }
            var pkiDir = Path.Combine(_currentUsb.RootDirectory.FullName, "pki");
            if (!File.Exists(Path.Combine(pkiDir, "cert.crt")) ||
                !File.Exists(Path.Combine(pkiDir, "priv.key")))
            {
                MessageBox.Show("Debes generar PKI primero.");
                return;
            }

            /* 1. Crear usuario -------------------------------------------------- */
            var dtoUser = new
            {
                rut = txtRut.Text.Trim(),
                nombre = txtNombre.Text.Trim(),
                depto = txtDepto.Text.Trim(),
                email = txtMail.Text.Trim(),
                rol = cmbRol.SelectedItem!.ToString(),
                pin = txtPin.Text.Trim()
            };
            System.Diagnostics.Debug.WriteLine("DTO Usuario ► " + JsonSerializer.Serialize(dtoUser));

            try
            {
                var resp = await _api.PostAsync<UsuarioCreatedResponse>("/api/usuarios", dtoUser);
                System.Diagnostics.Debug.WriteLine($"Respuesta /api/usuarios: id={resp.id}, msg={resp.msg}");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API /api/usuarios: {ex.Message}");
                return;
            }

            /* 2. Vincular USB --------------------------------------------------- */
            var dtoUsb = new { Serial = _serial, UsuarioRut = txtRut.Text.Trim() };
            System.Diagnostics.Debug.WriteLine("DTO USB ► " + JsonSerializer.Serialize(dtoUsb));

            try
            {
                await _api.PostAsync("/api/usb/asignar", dtoUsb);
                System.Diagnostics.Debug.WriteLine("USB asignado OK.");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API /api/usb/asignar: {ex.Message}");
                return;
            }

            /* 3. Copiar PKI + config al pendrive ------------------------------- */
            CopyDir(pkiDir, Path.Combine(_currentUsb.RootDirectory.FullName, "pki"));

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

            System.Diagnostics.Debug.WriteLine("Empleado + USB registrados ✔️");
            MessageBox.Show("Empleado + USB registrados ✔️");
            UsbPrepared?.Invoke(_serial);
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

        private void ToggleForm(bool enabled) =>
            btnGenPki.Enabled = btnCreate.Enabled = enabled;

        private void Log(string msg) =>
            logsTextBox.AppendText($"{DateTime.Now:HH:mm:ss}  {msg}{Environment.NewLine}");

        /* ===============================================================
           5. Tipos usados en la comunicación con backend
        ===============================================================*/
        private record UsuarioCreatedResponse(int id, string? msg);

        /* ===============================================================
           Métodos/Clases sin uso
        ===============================================================*/
        // La clase CrearUsuarioResponse ya no se usa; la respuesta se maneja con
        // record UsuarioCreatedResponse que refleja el JSON actual del backend.
        /*
        public class CrearUsuarioResponse
        {
            public int Id { get; set; }
        }
        */
    }
}
