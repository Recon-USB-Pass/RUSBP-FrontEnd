using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace rusbp_bootstrap.Core
{
    public static class PkiService
    {
        public static (string certPath, string keyPath) GeneratePkcs8KeyPair(string commonName, string destDir)
        {
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest(
                new X500DistinguishedName($"CN={commonName}"),
                rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5));

            string certPath = Path.Combine(destDir, "cert.crt");
            string keyPath = Path.Combine(destDir, "priv.key");

            File.WriteAllText(certPath, PemEncoding.Write("CERTIFICATE", cert.RawData));
            File.WriteAllText(keyPath, PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey()));

            return (certPath, keyPath);
        }
    }
}
