using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace RUSBP_Admin.Core.Services
{
    public static class PkiService
    {
        /// <summary>
        /// Genera par de claves RSA y un certificado autofirmado.
        /// Guarda cert.crt (X.509) y priv.key (clave PKCS#8) en el directorio destino.
        /// </summary>
        /// <param name="serial">Serial USB para CN</param>
        /// <param name="destDir">Directorio donde guardar los archivos</param>
        /// <returns>Ruta del certificado y de la clave</returns>
        public static (string certPath, string privPath) GeneratePkcs8KeyPair(string serial, string destDir)
        {
            Directory.CreateDirectory(destDir);

            string certPath = Path.Combine(destDir, "cert.crt");
            string privPath = Path.Combine(destDir, "priv.key");

            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest(
                new X500DistinguishedName($"CN={serial}"),
                rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5));

            // --- GUARDA EL CERTIFICADO EN FORMATO PEM ---
            File.WriteAllText(certPath, new string(PemEncoding.Write("CERTIFICATE", cert.RawData)));

            // --- GUARDA LA CLAVE PRIVADA EN FORMATO PEM PKCS#8 ---
            File.WriteAllText(privPath, new string(PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey())));

            return (certPath, privPath);
        }



    }
}
