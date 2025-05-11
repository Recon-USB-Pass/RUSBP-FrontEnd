using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms.Vistas;

public partial class UsbAssignmentView : UserControl
{
    /* ───── Eventos (opcional) ───── */
    public event Action<string /*serial*/>? UsbPrepared;

    /* ───── Estado interno ───── */
    private DriveInfo? _currentUsb;   // unidad detectada
    private string? _serial;       // idem
    private string? _workDir;      // temp para PKI antes de copiar

    public UsbAssignmentView() => InitializeComponent();

    /* ====================================================================
     * 1. DETECTAR USB
     * ====================================================================*/
    private void btnDetect_Click(object? s, EventArgs e)
    {
        _currentUsb = DriveInfo.GetDrives()
                               .FirstOrDefault(d => d.DriveType == DriveType.Removable && d.IsReady);

        if (_currentUsb is null)
        {
            Log("⚠️  No se encontró ninguna unidad USB.");
            return;
        }

        _serial = GetVolumeSerial(_currentUsb.RootDirectory.FullName.TrimEnd('\\'));
        _lblUsbName.Text = _currentUsb.VolumeLabel;
        _lblUsbSize.Text = $"{_currentUsb.TotalSize / 1_073_741_824} GB";

        Log($"USB detectado: {_currentUsb.Name}  Serial={_serial}");
        ToggleForm(true);
    }

    /* Lee el serial vía volumeID */
    private static string GetVolumeSerial(string driveLetter)
    {
        try
        {
            using var m = new System.Management.ManagementObject(
                $@"Win32_LogicalDisk.DeviceID=""{driveLetter}:""");
            m.Get();
            return m["VolumeSerialNumber"]?.ToString() ?? "UNKNOWN";
        }
        catch { return "UNKNOWN"; }
    }

    /* ====================================================================
     * 2. GENERAR PKI (local)
     * ====================================================================*/
    private void btnGenPki_Click(object? s, EventArgs e)
    {
        if (_currentUsb is null) return;

        _workDir = Path.Combine(Path.GetTempPath(), "UsbPrep", _serial ?? "tmp", "pki");
        Directory.CreateDirectory(_workDir);

        using var rsa = RSA.Create(2048);
        var csr = new CertificateRequest("CN=USB_USER", rsa, HashAlgorithmName.SHA256,
                                         RSASignaturePadding.Pkcs1);

        var cert = csr.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(2));

        File.WriteAllBytes(Path.Combine(_workDir, "cert.crt"), cert.Export(X509ContentType.Cert));
        File.WriteAllBytes(Path.Combine(_workDir, "priv.key"), rsa.ExportPkcs8PrivateKey());

        _icoCert.BackColor =
        _icoPriv.BackColor = Color.LimeGreen;

        Log("Claves PKI generadas.");
    }

    /* ====================================================================
     * 3. CREAR AMBIENTE DEL USB
     * ====================================================================*/
    private void btnCreate_Click(object? s, EventArgs e)
    {
        if (_currentUsb is null || _workDir is null)
        {
            MessageBox.Show("Primero detecta un USB y genera las claves.");
            return;
        }
        if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
            string.IsNullOrWhiteSpace(txtRut.Text) ||
            string.IsNullOrWhiteSpace(cmbRol.Text))
        {
            MessageBox.Show("Completa la información del empleado.");
            return;
        }

        // 1) Copiar PKI
        var destPki = Path.Combine(_currentUsb.RootDirectory.FullName, "pki");
        CopyDir(_workDir, destPki);

        // 2) Crear config.json
        var cfg = new
        {
            Nombre = txtNombre.Text,
            Rut = txtRut.Text,
            Depto = txtDepto.Text,
            Email = txtMail.Text,
            Rol = cmbRol.Text,
            Serial = _serial,
            Fecha = DateTime.Now
        };
        File.WriteAllText(Path.Combine(_currentUsb.RootDirectory.FullName, "config.json"),
                          JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));

        _icoCfg.BackColor = Color.LimeGreen;
        Log("Archivos copiados. Ambiente creado ✔️");

        UsbPrepared?.Invoke(_serial ?? "");
    }

    private static void CopyDir(string src, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var f in Directory.GetFiles(src))
            File.Copy(f, Path.Combine(dest, Path.GetFileName(f)), true);

        foreach (var d in Directory.GetDirectories(src))
            CopyDir(d, Path.Combine(dest, Path.GetFileName(d)));
    }

    /* ====================================================================
     * 4. Helpers UI
     * ====================================================================*/
    private void ToggleForm(bool enabled)
    {
        foreach (Control c in _grpInfo.Controls) c.Enabled = enabled;
        btnGenPki.Enabled = enabled;
        btnCreate.Enabled = enabled;
    }

    private void Log(string msg) => lstLog.Items.Add($"{DateTime.Now:HH:mm:ss}  {msg}");
}

