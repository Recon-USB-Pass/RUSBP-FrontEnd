using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Models.Dtos;
using System.Windows.Forms;

namespace RUSBP_Admin.Core.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(string baseUrl)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };

            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(20)
            };
        }

        public ApiClient(HttpClient http) => _http = http;

        private string U(string relative) => relative.StartsWith('/') ? relative : "/" + relative;

        // ──────────────── A: Usuarios ────────────────

        public async Task<UsuarioDto?> GetUsuarioAsync(int id)
            => await _http.GetFromJsonAsync<UsuarioDto>(U($"api/Usuarios/{id}"));

        public async Task<List<UsuarioDto>?> GetUsuariosAsync()
            => await _http.GetFromJsonAsync<List<UsuarioDto>>(U("api/Usuarios"));

        public async Task<UsuarioDto?> CrearUsuarioAsync(CrearDto dto)
        {
            var resp = await _http.PostAsJsonAsync(U("api/Usuarios"), dto);
            if (resp.IsSuccessStatusCode)
                return await resp.Content.ReadFromJsonAsync<UsuarioDto>();
            else
                throw new HttpRequestException(await resp.Content.ReadAsStringAsync(), null, resp.StatusCode);
        }

        // ──────────────── B: USBs ────────────────

        public async Task PostCrearUsbAsync(CrearUsbDto dto)
        {
            var resp = await _http.PostAsJsonAsync(U("api/usb"), dto);
            resp.EnsureSuccessStatusCode();
        }

        public async Task PostRegistrarRpAsync(RegisterDto dto)
        {
            var resp = await _http.PostAsJsonAsync(U("api/usb/register"), dto);
            resp.EnsureSuccessStatusCode();
        }

        public async Task PostAsignarUsbAsync(AsignarDto dto)
        {
            var resp = await _http.PostAsJsonAsync(U("api/usb/asignar"), dto);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<bool> LinkUsbAsync(string serial, string rut)
        {
            var resp = await _http.PostAsync(U($"api/usb/{serial}/link/{rut}"), null);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> IsUsbOnlineAsync(string serial)
        {
            var resp = await _http.GetAsync(U($"api/usb/{serial}/online"));
            if (!resp.IsSuccessStatusCode) return false;
            var str = await resp.Content.ReadAsStringAsync();
            return bool.TryParse(str, out var b) && b;
        }

        // ──────────────── C: Autenticación ────────────────

        public async Task<string?> VerifyUsbAsync(string serial, string certPem, CancellationToken ct = default)
        {
            var dto = new { serial, certPem };
            var resp = await _http.PostAsJsonAsync(U("api/auth/verify-usb"), dto, ct);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadAsStringAsync(ct) : null;
        }

        public async Task<RecoverResponseDto?> RecoverAsync(RecoverDto dto, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync(U("api/auth/recover"), dto, ct);
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<RecoverResponseDto>(json);
        }

        public async Task<(UsuarioDto? usuario, string? error)> LoginUsbAsync(string serial, string sig, string pin, string mac)
        {
            var dto = new RecoverDto(serial, sig, pin, 1, mac);
            var res = await RecoverAsync(dto);
            return res != null
                ? (res.Usuario, null)
                : (null, "PIN incorrecto o error de autenticación.");
        }

        // ──────────────── D: Logs ────────────────

        public async Task<List<LogActividadDto>> GetLogsAsync(int page = 1, int pageSize = 50)
        {
            var resp = await _http.GetFromJsonAsync<List<LogActividadDto>>(U($"api/logs?page={page}&pageSize={pageSize}"));
            return resp ?? new();
        }

        public async Task<bool> SendLogsAsync(IEnumerable<LogEventDto> events, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync(U("api/logs"), events, ct);
            return resp.IsSuccessStatusCode;
        }

        // ──────────────── E: Genéricos ────────────────

        public async Task<T> PostAsync<T>(string url, object body)
        {
            var resp = await _http.PostAsJsonAsync(U(url), body);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json)!;
        }

        public Task PostAsync(string url, object body)
            => _http.PostAsJsonAsync(U(url), body);

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            using var msg = new HttpRequestMessage(method, U(path));
            if (body != null)
            {
                msg.Content = new StringContent(JsonSerializer.Serialize(body),
                                                Encoding.UTF8, "application/json");
            }
            var resp = await _http.SendAsync(msg, ct);
            resp.EnsureSuccessStatusCode();
            return resp;
        }

        // ──────────────── Debug ────────────────

        private static void LogDebug(string msg)
        {
            try
            {
                string dir = Path.Combine(Path.GetTempPath(), "RUSBP", "logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "debug.txt"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {msg}{Environment.NewLine}");
            }
            catch { }
        }

        private static void MostrarErrorConexion(Exception ex)
        {
            if (ex is HttpRequestException ||
                ex.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("No hay conexión con el servidor.\nVerifique red o backend.",
                                "Sin conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show($"Error de conexión TLS:\n{ex.Message}",
                                "Error SSL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LogDebug($"[ApiClient] Conn ERROR {ex.Message}");
        }
    }
}
