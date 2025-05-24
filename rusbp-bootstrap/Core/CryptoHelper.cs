using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace rusbp_bootstrap.Core
{
    public static class CryptoHelper
    {
        public static byte[] EncryptString(string plainText, string password)
        {
            using var aes = Aes.Create();
            var salt = RandomNumberGenerator.GetBytes(16);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32);
            aes.Key = key;
            aes.GenerateIV();
            using var ms = new MemoryStream();
            ms.Write(salt); // 16 bytes
            ms.Write(aes.IV); // 16 bytes
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText);
            return ms.ToArray();
        }

        public static string DecryptString(byte[] cipherData, string password)
        {
            using var ms = new MemoryStream(cipherData);
            var salt = new byte[16];
            ms.Read(salt, 0, 16);
            var iv = new byte[16];
            ms.Read(iv, 0, 16);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
        /// <summary>
        /// Firma un challenge en base64 usando una clave privada RSA en formato PEM (PKCS#8 o PKCS#1).
        /// </summary>
        public static string FirmarChallenge(string privateKeyPem, string challengeBase64)
        {
            byte[] challengeBytes = Convert.FromBase64String(challengeBase64);

            using RSA rsa = RSA.Create();

            // Desde .NET 5 soporta PKCS#8. Si tu clave es PKCS#1, intenta actualizar a .NET 7+.
            try
            {
                rsa.ImportFromPem(privateKeyPem.ToCharArray());
            }
            catch (Exception)
            {
                throw new Exception("Error importando la clave privada. Asegúrate que esté en formato PEM (PKCS#8 o PKCS#1) y sin password.");
            }

            // Firma usando SHA256 y PKCS#1 v1.5 padding
            byte[] signature = rsa.SignData(challengeBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }
    }
}

