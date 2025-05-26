using System;
using System.IO;
using System.Linq;                  //  👈  ¡nuevo!
using System.Security.Cryptography;
using System.Text;

namespace rusbp_bootstrap.Core
{
    /// <summary>
    /// AES-GCM con IV fijo 0 y tag de 16 bytes.
    /// El buffer resultante se guarda    TAG(16) || CIPHER(n)
    /// </summary>
    public static class CryptoHelper
    {
        private static readonly byte[] IV = new byte[12]; // 96-bit nonce = 0
        private const int TAG_LEN = 16;

        /*──────────────────── 1) Encrypt string → byte[ tag | cipher ] ──*/
        public static byte[] EncryptString(string plain, string pass)
        {
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(pass));
            byte[] plainBytes = Encoding.UTF8.GetBytes(plain);
            byte[] cipher = new byte[plainBytes.Length];
            byte[] tag = new byte[TAG_LEN];

            //  ← patrón correcto: constructor por defecto (tag = 16 bytes)
            using var gcm = new AesGcm(key);

            gcm.Encrypt(IV, plainBytes, cipher, tag);
            return tag.Concat(cipher).ToArray();                // TAG || CIPHER
        }

        /*──────────────────── 2) Decrypt byte[] → string ───────────────*/
        public static string DecryptToString(byte[] tagCipher, string pass)
        {
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(pass));
            byte[] tag = tagCipher[..TAG_LEN];
            byte[] cipher = tagCipher[TAG_LEN..];
            byte[] plain = new byte[cipher.Length];

            using var gcm = new AesGcm(key);                    // tag = 16 bytes
            gcm.Decrypt(IV, cipher, tag, plain);

            return Encoding.UTF8.GetString(plain);
        }

        /*──────────────────── 3) Fragmentos en Base64 para backend ─────*/
        public static (string cipherB64, string tagB64) ToPartsBase64(byte[] tagCipher)
            => (Convert.ToBase64String(tagCipher[TAG_LEN..]),
                Convert.ToBase64String(tagCipher[..TAG_LEN]));

        /*────────────────────── Firma del challenge RSA ──────────────────*/
        public static string FirmarChallenge(string privateKeyPem, string challengeB64)
        {
            byte[] challenge = Convert.FromBase64String(challengeB64);
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            byte[] sig = rsa.SignData(challenge, HashAlgorithmName.SHA256,
                                      RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(sig);
        }
    }
}
