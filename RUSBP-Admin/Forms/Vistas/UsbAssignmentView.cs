// UsbAssignmentView.cs  (lógica)
// ─────────────────────────────────────────────────────────────────────────────
// Adaptado: 2025-05-22
//  • Se sustituye la obtención del serial usando WMI por llamada a Win32 API
//    GetVolumeInformation (kernel32), mucho más confiable en USBs protegidos.
//  • Se centraliza la lectura del serial en GetUsbSerial() y _serial sólo se
//    establece desde ese helper.
//  • Se comenta cualquier clase / método que ya no se usa y se explica “por qué”.
// ─────────────────────────────────────────────────────────────────────────────

using System.Management;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Forms;
using RUSBP_Admin.Core.Services;        // ApiClient  y  UsbCryptoService
using System.Security.Cryptography.X509Certificates;

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

        private static string GetUsbSerial(string driveLetter)
        {
            driveLetter = driveLetter.TrimEnd('\\', ':');
            Console.WriteLine($"[GetUsbSerial] Analizando letra: {driveLetter}");
            Debug.WriteLine($"[GetUsbSerial] Analizando letra: {driveLetter}");

            try
            {
                // 1. Obtener el DeviceID del disco asociado a la letra
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
                                Console.WriteLine($"[GetUsbSerial] DeviceID físico asociado: {targetDiskDeviceId}");
                                Debug.WriteLine($"[GetUsbSerial] DeviceID físico asociado: {targetDiskDeviceId}");
                                break;
                            }
                        }
                    }
                }

                // 2. Ahora busca el SerialNumber de ese DeviceID entre los USB
                using (var drives = new ManagementObjectSearcher("SELECT DeviceID, SerialNumber, InterfaceType FROM Win32_DiskDrive WHERE InterfaceType='USB'"))
                {
                    foreach (ManagementObject drive in drives.Get())
                    {
                        var devId = drive["DeviceID"]?.ToString() ?? "";
                        var sn = (drive["SerialNumber"]?.ToString() ?? "").Trim();
                        Console.WriteLine($"[GetUsbSerial] USB DeviceID={devId}, SerialNumber={sn}");
                        Debug.WriteLine($"[GetUsbSerial] USB DeviceID={devId}, SerialNumber={sn}");

                        if (devId == targetDiskDeviceId && !string.IsNullOrWhiteSpace(sn))
                        {
                            Console.WriteLine($"[GetUsbSerial] Serial USB ENCONTRADO: {sn.ToUpperInvariant()}");
                            Debug.WriteLine($"[GetUsbSerial] Serial USB ENCONTRADO: {sn.ToUpperInvariant()}");
                            return sn.ToUpperInvariant();
                        }
                    }
                }
                Console.WriteLine("[GetUsbSerial] No se encontró el serial en discos USB. Fallback: VolumeSerial");
                Debug.WriteLine("[GetUsbSerial] No se encontró el serial en discos USB. Fallback: VolumeSerial");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[GetUsbSerial] Excepción: " + ex.Message);
                Debug.WriteLine("[GetUsbSerial] Excepción: " + ex.Message);
            }

            // 3. Fallback: usar Volume Serial sólo si WMI falla completamente
            try
            {
                if (GetVolumeInformation($@"{driveLetter}:\", null, 0, out uint volSer, out _, out _, null, 0))
                {
                    var fallbackSerial = volSer.ToString("X8");
                    Console.WriteLine($"[GetUsbSerial] Fallback Volume Serial: {fallbackSerial}");
                    Debug.WriteLine($"[GetUsbSerial] Fallback Volume Serial: {fallbackSerial}");
                    return fallbackSerial;
                }
            }
            catch { /* Nada, solo fallback*/ }

            Console.WriteLine("[GetUsbSerial] Retornando 'UNKNOWN'");
            Debug.WriteLine("[GetUsbSerial] Retornando 'UNKNOWN'");
            return "UNKNOWN";
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool GetVolumeInformation(
        string lpRootPathName,
        System.Text.StringBuilder? lpVolumeNameBuffer,
        int nVolumeNameSize,
        out uint lpVolumeSerialNumber,
        out uint lpMaximumComponentLength,
        out uint lpFileSystemFlags,
        System.Text.StringBuilder? lpFileSystemNameBuffer,
        int nFileSystemNameSize);

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
            // 1. Validaciones previas
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

            // 2. Generar PKI en el USB (cert.crt y priv.key)
            if (!File.Exists(Path.Combine(pkiDir, "cert.crt")) || !File.Exists(Path.Combine(pkiDir, "priv.key")))
            {
                Directory.CreateDirectory(pkiDir);
                // Puedes usar tu PkiService estático, asegúrate que retorna cert en formato DER/PEM según backend
                var (certPath, privPath) = PkiService.GeneratePkcs8KeyPair(_serial, pkiDir);
                if (!File.Exists(certPath) || !File.Exists(privPath))
                {
                    MessageBox.Show("Error generando PKI en el USB.");
                    return;
                }
            }

            // 3. Calcular thumbprint del certificado generado
            string certFile = Path.Combine(pkiDir, "cert.crt");
            string thumbprint = "";
            try
            {
                // Si tu cert está en formato PEM, conviértelo primero a X509Certificate2 (soporta DER y PEM)
                var certBytes = File.ReadAllBytes(certFile);

                // Probar si es PEM
                string certText = File.ReadAllText(certFile);
                if (certText.Contains("BEGIN CERTIFICATE"))
                {
                    // Strip headers, decode base64, cargar como DER
                    string base64 = string.Join("", certText
                        .Split('\n')
                        .Where(line => !line.StartsWith("-----")));
                    certBytes = Convert.FromBase64String(base64.Trim());
                }

                var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certBytes);
                thumbprint = x509.Thumbprint ?? "";
                Debug.WriteLine($"[btnCreate] Thumbprint calculado: {thumbprint}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error leyendo cert.crt o calculando thumbprint: {ex.Message}");
                return;
            }

            // 4. Crear / actualizar usuario (igual que antes)
            var dtoUser = new
            {
                rut = txtRut.Text.Trim(),
                nombre = txtNombre.Text.Trim(),
                depto = txtDepto.Text.Trim(),
                email = txtMail.Text.Trim(),
                rol = cmbRol.SelectedItem?.ToString() ?? "Empleado",
                pin = txtPin.Text.Trim()
            };
            Debug.WriteLine("DTO Usuario ► " + JsonSerializer.Serialize(dtoUser));

            try
            {
                var resp = await _api.PostAsync<UsuarioCreatedResponse>("/api/usuarios", dtoUser);
                Debug.WriteLine($"Respuesta /api/usuarios: id={resp.id}, msg={resp.msg}");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API /api/usuarios: {ex.Message}");
                return;
            }

            // 5. Alta (o existencia) del USB, enviando el thumbprint
            var altaDto = new { serial = _serial, thumbprint = thumbprint };
            Debug.WriteLine("POST /api/usb  →  " + JsonSerializer.Serialize(altaDto));

            try
            {
                await _api.PostAsync("/api/usb", altaDto);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Debug.WriteLine("USB ya registrado (409 Conflict) – se continúa.");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API /api/usb: {ex.Message}");
                return;
            }

            // 6. Vincular USB ⇄ Usuario
            var vincDto = new { Serial = _serial, UsuarioRut = dtoUser.rut };
            Debug.WriteLine("POST /api/usb/asignar  →  " + JsonSerializer.Serialize(vincDto));

            try
            {
                await _api.PostAsync("/api/usb/asignar", vincDto);
                Debug.WriteLine("USB asignado OK.");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error API /api/usb/asignar: {ex.Message}");
                return;
            }

            // 7. Copiar PKI + escribir config.json (seguro que ya están, pero refresca config)
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

            Debug.WriteLine("Empleado + USB registrados ✔️");
            MessageBox.Show("Empleado + USB registrados ✔️");
            UsbPrepared?.Invoke(_serial);
        }


        /* ===============================================================
           4. helpers
        ===============================================================*/
        private static void CopyDir(string src, string dst)
        {
            Directory.CreateDirectory(dst);
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

    }
}
