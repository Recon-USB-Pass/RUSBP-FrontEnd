using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace RUSBP_Admin.Core.Services
{
    /// <summary>
    /// Genera un par de claves RSA-2048 y un certificado X.509 autofirmado
    /// (5 años) en formato PEM: <c>cert.crt</c> y <c>priv.key</c>.
    /// </summary>
    public static class PkiService
    {
        /// <param name="commonName">Valor que irá en el CN (usamos el serial del USB).</param>
        /// <param name="destDir">Carpeta donde se escribirán los archivos.</param>
        /// <returns>(rutaCert, rutaPrivKey)</returns>
        public static (string certPath, string keyPath) GeneratePkcs8KeyPair(
            string commonName, string destDir)
        {
            Directory.CreateDirectory(destDir);

            // ─ 1. Genera la clave y la CSR ─
            using var rsa = RSA.Create(2048);
            var req = new CertificateRequest(
                new X500DistinguishedName($"CN={commonName}"),
                rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // ─ 2. Certificado autofirmado 5 años ─
            using var cert = req.CreateSelfSigned(
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddYears(5));

            // ─ 3. Guarda PEM (idéntico a bootstrap) ─
            string certPath = Path.Combine(destDir, "cert.crt");
            string keyPath = Path.Combine(destDir, "priv.key");

            File.WriteAllText(certPath,new string(PemEncoding.Write("CERTIFICATE", cert.RawData)));
            File.WriteAllText(keyPath,new string(PemEncoding.Write("PRIVATE KEY",rsa.ExportPkcs8PrivateKey())));

            return (certPath, keyPath);
        }

        /// <summary>Devuelve el thumbprint (SHA-1) de un PEM.</summary>
        public static string GetThumbprintFromPem(string pemPath)
        {
            // Lee todo el fichero PEM
            var pem = File.ReadAllText(pemPath);

            // Elimina las cabeceras -----BEGIN/END----- y líneas vacías
            var b64 = string.Concat(
                         pem.Split(new[] { '\r', '\n' },
                                   StringSplitOptions.RemoveEmptyEntries)
                            .Where(l => !l.StartsWith("-----")));

            // Decodifica Base-64 → DER y crea el X509
            var raw = Convert.FromBase64String(b64);
            using var cert = new X509Certificate2(raw);

            return cert.Thumbprint!.ToUpperInvariant();
        }

    }
}
