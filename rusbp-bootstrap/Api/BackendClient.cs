using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace rusbp_bootstrap.Api
{
    public class BackendClient
    {
        private readonly HttpClient _http;

        public BackendClient(HttpClient http)
        {
            _http = http;
        }

        // 1. Registrar el USB en backend
        public async Task<bool> RegistrarUsbAsync(string serial, string thumbprint)
        {
            var usb = new UsbDto { Serial = serial, Thumbprint = thumbprint };
            var resp = await _http.PostAsJsonAsync("/api/usb", usb);
            return resp.IsSuccessStatusCode;
        }


        // 2. Crear usuario root-admin
        public async Task<UsuarioCreated?> CrearUsuarioAsync(UsuarioDto usuarioDto)
        {
            var resp = await _http.PostAsJsonAsync("/api/usuarios", usuarioDto);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<UsuarioCreated>();
        }

        // 3. Asociar USB <-> Usuario
        public async Task<bool> AsignarUsbAUsuarioAsync(string serial, string usuarioRut)
        {
            var vincDto = new VincularUsbDto { Serial = serial, UsuarioRut = usuarioRut };
            var resp = await _http.PostAsJsonAsync("/api/usb/asignar", vincDto);
            return resp.IsSuccessStatusCode;
        }

        // 4. Probar /api/auth/verify_usb
        public async Task<bool> ProbarVerifyUsbAsync(string serial)
        {
            // Si tu backend espera { serial: ... } u otro payload, ajústalo aquí
            var resp = await _http.PostAsJsonAsync("/api/auth/verify_usb", new { serial });
            if (!resp.IsSuccessStatusCode) return false;
            // Opcional: revisa respuesta body si quieres más info
            return true;
        }

        // 5. Probar /api/auth/login (ajusta payload según backend)
        public async Task<string?> ObtenerChallengeVerifyUsbAsync(string serial, string certPem)
        {
            var payload = new { serial, certPem };
            var resp = await _http.PostAsJsonAsync("/api/auth/verify-usb", payload);
            if (resp.IsSuccessStatusCode)
                return await resp.Content.ReadAsStringAsync();
            return null;
        }

        public async Task<bool> ProbarLoginAsync(string serial, string signatureBase64, string pin, string macAddress)
        {
            var payload = new { serial, signatureBase64, pin, macAddress };
            var resp = await _http.PostAsJsonAsync("/api/auth/login", payload);
            return resp.IsSuccessStatusCode;
        }


        // 6. ELIMINAR Usuario (por Rut)
        public async Task<bool> EliminarUsuarioAsync(string rut)
        {
            // Ajusta el endpoint según tu API
            var resp = await _http.DeleteAsync($"/api/usuarios/{rut}");
            return resp.IsSuccessStatusCode;
        }

        // 7. ELIMINAR USB (por Serial)
        public async Task<bool> EliminarUsbAsync(string serial)
        {
            // Ajusta el endpoint según tu API
            var resp = await _http.DeleteAsync($"/api/usb/{serial}");
            return resp.IsSuccessStatusCode;
        }
    }
}
