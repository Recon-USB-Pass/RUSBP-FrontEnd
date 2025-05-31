// UsbCryptoService.cs  –  v2  (usa pki/cert.crt + pki/priv.key)
// -----------------------------------------------------------------------------
// • Busca el primer USB que contenga pki/cert.crt  y  pki/priv.key
// • Devuelve el certificado en PEM (LoadCertPem)
// • Firma el challenge con la clave privada PKCS#8  (Sign)
// -----------------------------------------------------------------------------
// NOTA
// - cert.crt y priv.key siguen estando en **formato PEM**; sólo cambia el
//   nombre y la ubicación.  No es necesario convertir a DER.
// - La verificación del certificado se sigue haciendo en el backend mediante
//   el endpoint /api/auth/verify-usb  (LoginForm.BeginVerificationAsync).
// -----------------------------------------------------------------------------

using System.Management;
using System.Security.Cryptography;

namespace RUSBP_Admin.Core.Services
{
    public class UsbCryptoService
    {
        public string? MountedRoot { get; set; }
        public string? Serial { get; private set; }

        // Rutas relativas dentro del pendrive
        private const string CERT_REL = @"pki\cert.crt";
        private const string KEY_REL = @"pki\priv.key";

        /* ===============================================================
           1. Localiza el primer USB que contenga la carpeta /pki
        ===============================================================*/
        public bool TryLocateUsb()
        {
            foreach (var info in EnumerateUsbInfos())
            {
                foreach (var root in info.Roots)
                {
                    var driveLetter = root.Substring(0, 2);
                    var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.StartsWith(driveLetter));

                    // 🔒 Si la unidad no está lista, pide al usuario desbloquearla
                    if (drive == null || !drive.IsReady)
                    {
                        MessageBox.Show(
                            $"La unidad {driveLetter} está bloqueada por BitLocker. Debes desbloquearla antes de continuar.",
                            "Unidad Bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    string pkiDir = Path.Combine(root, "pki");
                    if (!Directory.Exists(pkiDir) || !File.Exists(Path.Combine(pkiDir, "cert.crt")) || !File.Exists(Path.Combine(pkiDir, "priv.key")))
                    {
                        MessageBox.Show(
                            "USB conectado, pero no contiene carpeta PKI (cert.crt / priv.key).",
                            "Falta PKI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    // Si llega aquí, todo está bien:
                    Serial = info.Serial.ToUpperInvariant();
                    MountedRoot = root;
                    //IsRoot = true;
                    return true;
                }
            }
            Serial = MountedRoot = null;
            //IsRoot = false;
            return false;
        }


        /* ===============================================================
           2. Lee el certificado PEM para enviarlo al backend
        ===============================================================*/
        public string LoadCertPem()
        {
            if (MountedRoot is null)
                throw new InvalidOperationException("USB no localizado.");
            var path = Path.Combine(MountedRoot, CERT_REL);
            LogDebug($"[LoadCertPem] Leyendo certificado: {path}");
            var cert = File.ReadAllText(path);
            LogDebug($"[LoadCertPem] Certificado leído correctamente.");
            return cert;
        }

        /* ===============================================================
           3. Firma el challenge con la clave privada PKCS#8
        ===============================================================*/
        public string Sign(string challengeB64)
        {
            if (MountedRoot is null)
                throw new InvalidOperationException("USB no localizado.");

            byte[] challenge = Convert.FromBase64String(challengeB64);
            string keyPath = Path.Combine(MountedRoot, KEY_REL);
            LogDebug($"[Sign] Leyendo clave privada: {keyPath}");

            // Leer la clave en scope controlado:
            string keyPem = File.ReadAllText(keyPath);
            LogDebug($"[Sign] Clave privada leída correctamente.");

            using var rsa = RSA.Create();
            rsa.ImportFromPem(keyPem);

            byte[] sig = rsa.SignData(challenge,
                                      HashAlgorithmName.SHA256,
                                      RSASignaturePadding.Pkcs1);

            LogDebug($"[Sign] Challenge firmado correctamente.");
            return Convert.ToBase64String(sig);
        }

        /* ===============================================================
           Helpers internos
        ===============================================================*/
        public record UsbInfo(string Serial, List<string> Roots);

        // Devuelve serial + raíces de todos los discos USB conectados
        public static List<UsbInfo> EnumerateUsbInfos()
        {
            var list = new List<UsbInfo>();

            var q = new ManagementObjectSearcher(
                "SELECT DeviceID, SerialNumber " +
                "FROM Win32_DiskDrive WHERE InterfaceType='USB'");

            foreach (ManagementObject d in q.Get())
            {
                string serial = d["SerialNumber"]?.ToString()?.Trim() ?? "";
                if (serial == "") continue;

                var roots = new List<string>();
                foreach (ManagementObject part in d.GetRelated("Win32_DiskPartition"))
                    foreach (ManagementObject log in part.GetRelated("Win32_LogicalDisk"))
                        roots.Add(log["DeviceID"] + "\\");

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
            catch
            {
                /* Ignora errores de logging */
            }
        }
        public static string SignWithKey(string privateKeyPem, string challengeB64)
        {
            byte[] challenge = Convert.FromBase64String(challengeB64);
            using var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportFromPem(privateKeyPem);
            byte[] sig = rsa.SignData(challenge, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(sig);
        }
    }
}





/*

// UsbCryptoService.cs  –  v2  (usa pki/cert.crt + pki/priv.key)
// -----------------------------------------------------------------------------
// • Busca el primer USB que contenga pki/cert.crt  y  pki/priv.key
// • Devuelve el certificado en PEM (LoadCertPem)
// • Firma el challenge con la clave privada PKCS#8  (Sign)
// -----------------------------------------------------------------------------
// NOTA
// - cert.crt y priv.key siguen estando en **formato PEM**; sólo cambia el
//   nombre y la ubicación.  No es necesario convertir a DER.
// - La verificación del certificado se sigue haciendo en el backend mediante
//   el endpoint /api/auth/verify-usb  (LoginForm.BeginVerificationAsync).
// -----------------------------------------------------------------------------

using System.Management;
using System.Security.Cryptography;

namespace RUSBP_Admin.Core.Services
{
    public class UsbCryptoService
    {
        public string? MountedRoot { get; private set; }
        public string? Serial { get; private set; }

        private const string CERT_REL = @"pki\cert.crt";
        private const string KEY_REL = @"pki\priv.key";

        /* 1) Localiza el primer USB con la estructura /pki *//*
        public bool TryLocateUsb()
        {
            var infos = EnumerateUsbInfos();

            if (infos.Count == 0)
            {
                LogDebug("No se detectaron USB conectados");
                return false;
            }

            foreach (var info in infos)
            {
                foreach (var root in info.Roots)
                {
                    string certPath = Path.Combine(root, CERT_REL);
                    string keyPath = Path.Combine(root, KEY_REL);

                    LogDebug($"Inspeccionando: {root}");
                    if (File.Exists(certPath)) LogDebug(" → cert.crt encontrado");
                    if (File.Exists(keyPath)) LogDebug(" → priv.key encontrado");

                    if (File.Exists(certPath) && File.Exists(keyPath))
                    {
                        Serial = info.Serial.ToUpperInvariant();
                        MountedRoot = root;

                        LogDebug($"USB válido ► Serial={Serial}  Root={MountedRoot}");
                        return true;
                    }
                }
            }

            MessageBox.Show($"USB conectado, pero no contiene la carpeta PKI ({CERT_REL}, {KEY_REL}).");
            return false;
        }

        /* 2) Lee el certificado PEM para enviarlo al backend *//*
        public string LoadCertPem()
        {
            string certPath = Path.Combine(MountedRoot!, CERT_REL);
            return File.ReadAllText(certPath);
        }

        /* 3) Firma el reto con la clave privada *//*
        public string Sign(string challengeB64)
        {
            byte[] challenge = Convert.FromBase64String(challengeB64);
            string keyPem = File.ReadAllText(Path.Combine(MountedRoot!, KEY_REL));

            using var rsa = RSA.Create();
            rsa.ImportFromPem(keyPem);

            byte[] sig = rsa.SignData(challenge, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(sig);
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

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
                        roots.Add(log["DeviceID"] + "\\");

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
            catch { /* ignora errores de logging *//* }
        }
    }
}

*/


