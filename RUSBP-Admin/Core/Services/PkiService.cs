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

            var certPath = Path.Combine(destDir, "cert.crt");
            var privPath = Path.Combine(destDir, "priv.key");

            using var rsa = RSA.Create(2048);
            var csr = new CertificateRequest($"CN={serial}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var cert = csr.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(2));
            File.WriteAllBytes(certPath, cert.Export(X509ContentType.Cert));
            File.WriteAllBytes(privPath, rsa.ExportPkcs8PrivateKey());

            return (certPath, privPath);
        }
    }
}
