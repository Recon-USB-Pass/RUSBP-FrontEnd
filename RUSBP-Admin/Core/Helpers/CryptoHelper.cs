using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RUSBP_Admin.Core.Helpers
{
    /// <summary>
    /// Ayuda para operaciones criptográficas: firma, cifrado y descifrado con PKI y archivos de configuración (.btlk, .btlk-ip).
    /// </summary>
    public static class CryptoHelper
    {
        // --- DESCIFRADO .btlk y .btlk-ip (AES-GCM) ---

        /// <summary>
        /// Desencripta un archivo tipo .btlk o .btlk-ip (AES-GCM, nonce de 12 bytes en cero, tag de 16 bytes al inicio).
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
        /// Desencripta el archivo .btlk-ip para extraer la IP cifrada (igual que DecryptBtlk).
        /// </summary>
        public static string DecryptBtlkIp(string filePath, string recoveryPass)
            => DecryptBtlk(filePath, recoveryPass);

        /// <summary>
        /// Deriva una clave AES-256 de 32 bytes a partir de la password (SHA256).
        /// </summary>
        public static byte[] DeriveKeyFromPass(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        // --- CIFRADO AES-GCM para compatibilidad si lo necesitas en el futuro ---

        /// <summary>
        /// Cifra texto en AES-GCM con clave derivada. Devuelve (cipher, tag).
        /// </summary>
        public static (byte[] cipher, byte[] tag) EncryptAesGcm(string plainText, byte[] key)
        {
            using var aes = new AesGcm(key);
            byte[] nonce = new byte[12]; // Fijo a cero para compatibilidad
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[16];
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
            return (cipherBytes, tag);
        }

        /// <summary>
        /// Descifra bytes usando AES-GCM.
        /// </summary>
        public static string DecryptAesGcm(byte[] cipher, byte[] tag, byte[] key)
        {
            using var aes = new AesGcm(key);
            byte[] nonce = new byte[12];
            byte[] plain = new byte[cipher.Length];
            aes.Decrypt(nonce, cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }


        // --- ÚTILES DE TEXTO ---

        public static string ToBase64(byte[] data) => Convert.ToBase64String(data);
        public static byte[] FromBase64(string b64) => Convert.FromBase64String(b64);

        // --- FIRMA DIGITAL Y PKI (por si mantienes validación de certificados, no borres estas) ---

        public static byte[] SignData(byte[] data, X509Certificate2 signerCert)
        {
            using var rsa = signerCert.GetRSAPrivateKey();
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public static bool VerifySignature(byte[] data, byte[] signature, X509Certificate2 cert)
        {
            using var rsa = cert.GetRSAPublicKey();
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        // --- CARGA DE CERTIFICADOS X.509 DESDE PEM O PFX ---

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

        // --- GESTIÓN DE BLOQUEO/ DESBLOQUEO DE UNIDADES CON BITLOCKER ---
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
        public static bool UnlockBitLockerWithRecoveryPass(string driveLetter, string recoveryPassword)
        {
            try
            {
                // Normalizar la letra (asegúrate que sea "G:")
                string normalized = driveLetter.Trim().TrimEnd('\\').TrimEnd(':') + ":";
                string args = $"-unlock {normalized} -RecoveryPassword {recoveryPassword}";
                string cmd = $"manage-bde {args}";

                Console.WriteLine("BITLOCKER CMD: " + cmd);
                Debug.WriteLine("BITLOCKER CMD: " + cmd);

                var psi = new ProcessStartInfo
                {
                    FileName = "manage-bde", // NO .exe
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas" // Garantiza que pida permisos de admin si es necesario
                };
                using var p = Process.Start(psi);
                p.WaitForExit();

                Console.WriteLine("STDOUT: " + p.StandardOutput.ReadToEnd());
                Console.WriteLine("STDERR: " + p.StandardError.ReadToEnd());
                Debug.WriteLine("ExitCode: " + p.ExitCode);

                return p.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Excepción al ejecutar manage-bde: " + ex);
                Debug.WriteLine("Excepción al ejecutar manage-bde: " + ex);
                return false;
            }
        }
    }
}
