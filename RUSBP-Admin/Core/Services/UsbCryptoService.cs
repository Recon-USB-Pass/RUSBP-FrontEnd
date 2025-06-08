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

using RUSBP_Admin.Forms;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using RUSBP_Admin.Core.Helpers;

namespace RUSBP_Admin.Core.Services
{
    public class UsbCryptoService
    {
        public string? MountedRoot { get; set; }
        public string? Serial { get; private set; }
        public bool IsRoot { get; private set; }

        // Rutas relativas dentro del pendrive
        private const string CERT_REL = @"pki\cert.crt";
        private const string KEY_REL = @"pki\priv.key";

        /* ===============================================================
           1. Localiza el primer USB que contenga la carpeta /pki
        ===============================================================*/
        public bool TryLocateUsb(bool esModoRoot = false)
        {
            foreach (var info in EnumerateUsbInfos())
            {
                foreach (var root in info.Roots)
                {
                    string pkiDir = Path.Combine(root, "pki");

                    // --- Si la unidad no está inicializada/cifrada ---
                    if (!Directory.Exists(pkiDir))
                    {
                        string driveLetter = root[..2]; // “F:”

                        if (!string.IsNullOrWhiteSpace(RpRootGlobal))
                        {
                            // Modo empleado: solo intenta unlock silencioso.
                            if (CryptoHelper.UnlockBitLockerWithRecoveryPass(driveLetter, RpRootGlobal, out _))
                            {
                                Thread.Sleep(2500);
                                if (!Directory.Exists(pkiDir)) continue;
                            }
                        }
                        else if (esModoRoot)
                        {
                            // Modo root: puede pedir el recovery password
                            if (!Prompt.ForRecoveryPassword(out string rp))
                                continue; // canceló

                            RpRootGlobal = rp;
                            if (!CryptoHelper.UnlockBitLockerWithRecoveryPass(driveLetter, rp, out _))
                            {
                                MessageBox.Show("No se pudo desbloquear el USB.",
                                    "Error BitLocker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                continue;
                            }
                            Thread.Sleep(2500);
                            if (!Directory.Exists(pkiDir)) continue;
                        }
                        else
                        {
                            // Modo empleado y no se puede desbloquear: NO poppear nada.
                            continue;
                        }
                    }

                    // Estructura OK
                    string sysDir = Path.Combine(root, "rusbp.sys");
                    bool hasRoot = File.Exists(Path.Combine(sysDir, ".btlk")) &&
                                   File.Exists(Path.Combine(sysDir, ".btlk-agente"));

                    Serial = info.Serial.ToUpperInvariant();
                    MountedRoot = root;
                    IsRoot = hasRoot;
                    return true;
                }
            }
            Serial = MountedRoot = null;
            IsRoot = false;
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

        /// <summary>
        /// Busca entre TODOS los USB bloqueados y solo retorna el que,
        /// tras desbloquear, realmente contiene la carpeta /pki y los archivos necesarios.
        /// </summary>
        public static (UsbInfo usb, string root)? FindFirstUsbWithPkiAfterUnlock(Func<string, string, Task<(bool ok, string? err)>> unlockFunc)
        {
            foreach (var usb in EnumerateUsbInfos())
            {
                foreach (var root in usb.Roots)
                {
                    string driveLetter = root[..2]; // Ej: F:
                    string pkiDir = Path.Combine(root, "pki");
                    string certPath = Path.Combine(pkiDir, "cert.crt");
                    string keyPath = Path.Combine(pkiDir, "priv.key");

                    // Solo si la unidad está bloqueada intentamos desbloquear
                    if (BitLockerService.IsLocked(driveLetter))
                    {
                        var unlockResult = unlockFunc(driveLetter, usb.Serial).GetAwaiter().GetResult();
                        if (!unlockResult.ok)
                            continue; // Si no se pudo desbloquear, sigue al siguiente USB
                    }

                    // Verificar que existe la PKI (ya desbloqueada)
                    if (Directory.Exists(pkiDir) &&
                        File.Exists(certPath) &&
                        File.Exists(keyPath))
                    {
                        return (usb, root);
                    }
                }
            }
            return null;
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
       
        public static string? RpRootGlobal { get; set; }

    }
}






