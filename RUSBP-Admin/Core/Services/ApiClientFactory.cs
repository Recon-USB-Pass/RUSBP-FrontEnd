using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace RUSBP_Admin.Core.Services
{
    public static class ApiClientFactory
    {
        public static ApiClient Create(string baseUrl)
        {
            /* Modo simple (HTTP) */
            return new ApiClient(baseUrl);

            /* ─ Modo HTTPS + PFX (desactiva el return de arriba cuando lo uses) ─
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(
                new X509Certificate2(@"E:\cert\usb_cert.pfx", "1234",
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet));

            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;

            return new ApiClient(new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            });
            */
        }
    }
}
