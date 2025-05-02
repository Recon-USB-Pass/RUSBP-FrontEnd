using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RUSBP_Admin.Core.Models.Dtos;

namespace RUSBP_Admin.Core.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        /* ──── Dos constructores ──── */
        public ApiClient(string baseUrl)
            : this(new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            })
        { }

        public ApiClient(HttpClient http) => _http = http;

        /* ──── Endpoints ──── */
        public Task LoginAsync(int usuarioId, string pin, string mac) =>
            _http.PostAsJsonAsync("/api/Auth/login",
                new LoginRequestDto(usuarioId, pin, mac));

        public Task<UsuarioDto?> GetUsuarioAsync(int id) =>
            _http.GetFromJsonAsync<UsuarioDto>($"/api/Usuarios/{id}");

        public Task AsignarUsbAsync(UsbAsignarRequestDto dto) =>
            _http.PostAsJsonAsync("/api/Usb/asignar", dto);

        public async Task<List<LogActividadDto>> GetLogsAsync(int page = 1, int pageSize = 50)
        {
            var res = await _http.GetFromJsonAsync<List<LogActividadDto>>(
                          $"/api/Logs?page={page}&pageSize={pageSize}");
            return res ?? [];
        }
    }
}
