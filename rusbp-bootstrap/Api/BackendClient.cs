// BackendClient.cs ─ rusbp-bootstrap
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using rusbp_bootstrap.Models;

namespace rusbp_bootstrap.Api
{
    /// <summary>
    /// Cliente muy fino para los endpoints reales que hoy expone el backend.
    /// Sólo conserva las llamadas que se usan en el flujo de bootstrap.  
    /// — 2025-05-26
    /// </summary>
    public sealed class BackendClient
    {
        private readonly HttpClient _http;
        public BackendClient(HttpClient http) => _http = http;

        /*────────────────────────────── 1) USB ──────────────────────────────*/

        /// <summary> POST /api/usb/register  (registra un USB con su rol) </summary>
        public async Task<bool> RegistrarUsbAsync(string serial,
                                                  byte[] cipher, byte[] tag,
                                                  UsbRole rol)
        {
            var body = new
            {
                serial,
                cipher = Convert.ToBase64String(cipher),
                tag = Convert.ToBase64String(tag),
                rol = (int)rol        // 0 = Root, 1 = Admin, 2 = Employee
            };
            var rsp = await _http.PostAsJsonAsync("/api/usb/register", body);
            string msg = await rsp.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG backend]: {msg}");
            return rsp.IsSuccessStatusCode;

        }

        /// <summary> POST /api/usb/asignar  (serial ←→ rut) </summary>
        public async Task<bool> AsignarUsbAUsuarioAsync(string serial, string usuarioRut)
        {
            var rsp = await _http.PostAsJsonAsync("/api/usb/asignar",
                                                  new { serial, usuarioRut });
            return rsp.IsSuccessStatusCode;
        }

        /*────────────────────────────── 2) USUARIOS ─────────────────────────*/

        /// <summary> POST /api/Usuarios </summary>
        public async Task<UsuarioCreated?> CrearUsuarioAsync(UsuarioDto dto)
        {
            var rsp = await _http.PostAsJsonAsync("/api/Usuarios", dto);
            return rsp.IsSuccessStatusCode
                 ? await rsp.Content.ReadFromJsonAsync<UsuarioCreated>()
                 : null;
        }

        /*────────────────────────────── 3) AUTH ─────────────────────────────*/

        /// <summary> POST /api/auth/verify-usb – devuelve el *challenge* Base64 </summary>
        public async Task<string?> ObtenerChallengeAsync(string serial, string certPem)
        {
            var rsp = await _http.PostAsJsonAsync("/api/auth/verify-usb",
                                                  new { serial, certPem });
            return rsp.IsSuccessStatusCode
                 ? await rsp.Content.ReadAsStringAsync()
                 : null;
        }

        /// <summary>
        /// POST /api/auth/recover  
        /// Realiza la prueba de autenticación completa (USB + PIN).  
        /// <paramref name="agentType"/> → 0 = Root / Admin-USB, 2 = Employee.
        /// </summary>
        public async Task<bool> ProbarRecoverAsync(string serial,
                                                   string signatureB64,
                                                   string pin,
                                                   int agentType = 0)
        {
            var body = new
            {
                serial,
                signatureBase64 = signatureB64,
                pin,
                agentType = agentType
            };
            var rsp = await _http.PostAsJsonAsync("/api/auth/recover", body);
            return rsp.IsSuccessStatusCode;
        }

        /*────────────────────────────── 4) Roll-back (best-effort) ──────────*/

        public Task<bool> EliminarUsbAsync(string serial)
            => _http.DeleteAsync($"/api/usb/{serial}")
                    .ContinueWith(t => t.Result.IsSuccessStatusCode);

        public Task<bool> EliminarUsuarioAsync(string rut)
            => _http.DeleteAsync($"/api/Usuarios/{rut}")
                    .ContinueWith(t => t.Result.IsSuccessStatusCode);
    }
}
