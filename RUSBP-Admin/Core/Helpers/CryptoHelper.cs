using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RUSBP_Admin.Core.Helpers
{
    /// <summary>
    /// Utilidades criptográficas: cifrado/descifrado AES-GCM, firma digital y carga de certificados.
    /// Compatible con el flujo del bootstrap (TAG de 16 bytes al inicio, IV=0, sin sal ni PBKDF2).
    /// </summary>
    public static class CryptoHelper
    {
        // --- CIFRADO AES-GCM PARA RP_x Y RP_ROOT (flujos bootstrap/admin) ---

        /// <summary>
        /// Cifra texto en AES-GCM (clave derivada SHA256, IV fijo 0), retorna (cipher, tag).
        /// </summary>
        public static (byte[] cipher, byte[] tag) EncryptAesGcm(string plainText, byte[] key)
        {
            using var aes = new AesGcm(key);
            byte[] nonce = new byte[12]; // IV todo cero
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[16];
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
            return (cipherBytes, tag);
        }

        /// <summary>
        /// Descifra texto AES-GCM (misma convención del bootstrap)
        /// </summary>
        public static string DecryptAesGcm(byte[] cipher, byte[] tag, byte[] key)
        {
            using var aes = new AesGcm(key);
            byte[] nonce = new byte[12];
            byte[] plain = new byte[cipher.Length];
            aes.Decrypt(nonce, cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }

        /// <summary>
        /// Deriva clave AES-256 a partir de una password (SHA256).
        /// </summary>
        public static byte[] DeriveKeyFromPass(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        /// <summary>
        /// Descifra .btlk o .btlk-ip (TAG de 16 bytes al inicio, IV=0).
        /// </summary>
        public static string DecryptBtlk(string filePath, string recoveryPass)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            if (fileBytes.Length < 16)
                throw new Exception(".btlk(.ip) corrupto o muy corto.");

            byte[] tag = new byte[16];
            Buffer.BlockCopy(fileBytes, 0, tag, 0, 16);
            byte[] cipher = new byte[fileBytes.Length - 16];
            Buffer.BlockCopy(fileBytes, 16, cipher, 0, cipher.Length);

            byte[] key = DeriveKeyFromPass(recoveryPass);
            byte[] nonce = new byte[12]; // 12 bytes a cero

            using var aes = new AesGcm(key);
            byte[] plaintext = new byte[cipher.Length];
            aes.Decrypt(nonce, cipher, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }

        /// <summary>
        /// Descifra .btlk-ip (alias de DecryptBtlk)
        /// </summary>
        public static string DecryptBtlkIp(string filePath, string recoveryPass)
            => DecryptBtlk(filePath, recoveryPass);

        /// <summary>
        /// Para compatibilidad: descifra buffer (TAG(16) || CIPHER(n)) en memoria.
        /// </summary>
        public static string DecryptToString(byte[] tagCipher, string pass)
        {
            const int TAG_LEN = 16;
            byte[] key = DeriveKeyFromPass(pass);
            byte[] tag = tagCipher[..TAG_LEN];
            byte[] cipher = tagCipher[TAG_LEN..];
            byte[] plain = new byte[cipher.Length];

            using var gcm = new AesGcm(key);
            gcm.Decrypt(new byte[12], cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }

        // --- FIRMA DIGITAL Y VALIDACIÓN (PKI, flujo de bootstrap y login challenge) ---

        /// <summary>
        /// Firma un bloque de datos con una clave privada PEM (challenge-response).
        /// </summary>
        public static string SignWithPemKey(string privateKeyPem, string challengeBase64)
        {
            byte[] challenge = Convert.FromBase64String(challengeBase64);
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            byte[] sig = rsa.SignData(challenge, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(sig);
        }

        /// <summary>
        /// Firma datos usando el certificado X509 (usado para testing/validación interna).
        /// </summary>
        public static byte[] SignData(byte[] data, X509Certificate2 signerCert)
        {
            using var rsa = signerCert.GetRSAPrivateKey();
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// Verifica firma digital usando certificado X509.
        /// </summary>
        public static bool VerifySignature(byte[] data, byte[] signature, X509Certificate2 cert)
        {
            using var rsa = cert.GetRSAPublicKey();
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        // --- UTILIDADES DE CERTIFICADOS (carga, conversión PEM/PFX) ---

        public static X509Certificate2 LoadCertificate(string path, string? password = null)
        {
            if (path.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase))
                return new X509Certificate2(path, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

            if (path.EndsWith(".pem", StringComparison.OrdinalIgnoreCase))
                return X509Certificate2.CreateFromPemFile(path);

            throw new NotSupportedException("Solo .pfx y .pem soportados");
        }

        public static X509Certificate2 FromPemString(string pem)
        {
            return X509Certificate2.CreateFromPem(pem);
        }

        // --- GESTIÓN DE BLOQUEO/DESBLOQUEO DE UNIDADES BITLOCKER ---
        public static bool LockDrive(string driveLetter)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "manage-bde",
                    Arguments = $"-lock {driveLetter}: -ForceDismount",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p.WaitForExit();
                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool DecryptDrive(string driveLetter)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "manage-bde",
                    Arguments = $"-off {driveLetter}:",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p.WaitForExit();
                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool UnlockBitLockerWithRecoveryPass(string driveLetter, string recoveryPassword, out string outputMsg)
        {
            outputMsg = "";
            try
            {
                string normalized = driveLetter.Trim().TrimEnd('\\').TrimEnd(':') + ":";
                string args = $"-unlock {normalized} -RecoveryPassword {recoveryPassword}";

                var psi = new ProcessStartInfo
                {
                    FileName = "manage-bde.exe",
                    Arguments = args,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit();

                outputMsg = output + error;

                if (p.ExitCode == 0 ||
                    output.Contains("ya está desbloqueado", StringComparison.OrdinalIgnoreCase) ||
                    error.Contains("ya está desbloqueado", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("already unlocked", StringComparison.OrdinalIgnoreCase) ||
                    error.Contains("already unlocked", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                outputMsg = ex.Message;
                return false;
            }
        }




        // --- UTILS DE TEXTO Y BASE64 ---
        public static string ToBase64(byte[] data) => Convert.ToBase64String(data);
        public static byte[] FromBase64(string b64) => Convert.FromBase64String(b64);
    }
}
