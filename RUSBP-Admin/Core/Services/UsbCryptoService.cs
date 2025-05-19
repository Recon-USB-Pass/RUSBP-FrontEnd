using System.Management;
using System.Security.Cryptography;

namespace RUSBP_Admin.Core.Services;

public class UsbCryptoService
{
    public string? MountedRoot { get; private set; }
    public string? Serial { get; private set; }

    /*  Locate first USB that contains device_cert.pem + device_key.pem */
    public bool TryLocateUsb()
    {
        var infos = EnumerateUsbInfos();
        const string query ="SELECT PNPDeviceID FROM Win32_DiskDrive WHERE InterfaceType='USB'";
        if (infos.Count == 0)
        {
            LogDebug("No se detectaron USB conectados");
            return false;
        }

        foreach (var info in infos)
        {
            foreach (var root in info.Roots)
            {
                string cert = Path.Combine(root, "device_cert.pem");
                string key = Path.Combine(root, "device_key.pem");

                LogDebug($"Inspeccionando: {root}");
                if (File.Exists(cert)) LogDebug(" → Certificado encontrado");
                if (File.Exists(key)) LogDebug(" → Clave privada encontrada");

                if (File.Exists(cert) && File.Exists(key))
                {
                    Serial = info.Serial.ToUpperInvariant();  // 🔥 Fuerza el casing correcto

                    MountedRoot = root;

                    LogDebug($"USB válido detectado - Serial: {Serial}, Root: {MountedRoot}");
                    return true;
                }
            }
        }

        MessageBox.Show("USB conectado, pero no contiene device_cert.pem o device_key.pem.");
        return false;
    }

    public string LoadCertPem() =>
        File.ReadAllText(Path.Combine(MountedRoot!, "device_cert.pem"));

    public string Sign(string challengeB64)
    {
        byte[] challenge = Convert.FromBase64String(challengeB64);
        string keyPem = File.ReadAllText(Path.Combine(MountedRoot!, "device_key.pem"));

        using var rsa = RSA.Create();
        rsa.ImportFromPem(keyPem);
        byte[] sig = rsa.SignData(challenge, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(sig);
    }

    /* ───────────── Helpers ───────────── */

    private record UsbInfo(string Serial, List<string> Roots);

    private static List<UsbInfo> EnumerateUsbInfos()
    {
        var list = new List<UsbInfo>();
        var q = new ManagementObjectSearcher(
            "SELECT DeviceID, SerialNumber FROM Win32_DiskDrive WHERE InterfaceType='USB'");

        foreach (ManagementObject d in q.Get())
        {
            string serial = d["SerialNumber"]?.ToString()?.Trim() ?? "";
            if (string.IsNullOrEmpty(serial)) continue;

            var roots = new List<string>();
            foreach (ManagementObject part in d.GetRelated("Win32_DiskPartition"))
                foreach (ManagementObject log in part.GetRelated("Win32_LogicalDisk"))
                    roots.Add(log["DeviceID"].ToString() + "\\");

            list.Add(new UsbInfo(serial, roots));
        }
        return list;
    }

    private static void LogDebug(string msg)
    {
        try
        {
            string dir = Path.Combine(Path.GetTempPath(), "RUSBP", "logs");
            Directory.CreateDirectory(dir);
            string ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText(Path.Combine(dir, "debug.txt"),
                               $"{ts} - {msg}{Environment.NewLine}");
        }
        catch { /* ignore */ }
    }

}


