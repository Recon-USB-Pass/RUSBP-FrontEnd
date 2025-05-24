using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

        // Registrar USB en backend
        public async Task<bool> RegistrarUsbAsync(string serial, string thumbprint = "")
        {
            var usbDto = new UsbDto { Serial = serial, Thumbprint = thumbprint };
            var resp = await _http.PostAsJsonAsync("/api/usb", usbDto);
            return resp.IsSuccessStatusCode;
        }

        // Crear usuario root-admin en backend
        public async Task<UsuarioCreated?> CrearUsuarioAsync(UsuarioDto usuario)
        {
            var resp = await _http.PostAsJsonAsync("/api/usuarios", usuario);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<UsuarioCreated>();
        }

        // Asociar USB a usuario
        public async Task<bool> AsignarUsbAUsuarioAsync(string serial, string usuarioRut)
        {
            var dto = new VincularUsbDto { Serial = serial, UsuarioRut = usuarioRut };
            var resp = await _http.PostAsJsonAsync("/api/usb/asignar", dto);
            return resp.IsSuccessStatusCode;
        }

        // (Opcional) Métodos para otros endpoints o manejo avanzado
    }
}
