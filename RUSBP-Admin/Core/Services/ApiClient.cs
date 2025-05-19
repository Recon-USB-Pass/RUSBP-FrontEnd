using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Models.Dtos;

namespace RUSBP_Admin.Core.Services
{
    /// <summary>
    /// Cliente HTTP único para TODOS los end-points (empleado + administrador).
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _http;

        /* ───────────── Actor ───────────── */

        public ApiClient(string baseUrl)
        {
            var handler = new HttpClientHandler
            {
                // ⚠ SOLO para desarrollo: acepta cualquier cert
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };

            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        // Para tests DI
        public ApiClient(HttpClient http) => _http = http;

        /* ══════════════════════════════════
           SECTION A · End-points ADMIN
           ══════════════════════════════════ */

        public Task LoginUserAsync(int usuarioId, string pin, string mac) =>
            _http.PostAsJsonAsync("/api/Auth/login",
                new LoginRequestDto(usuarioId, pin, mac));

        public Task<UsuarioDto?> GetUsuarioAsync(int id) =>
            _http.GetFromJsonAsync<UsuarioDto>($"/api/Usuarios/{id}");

        public Task<List<Employee>?> GetEmployeesAsync() =>
            _http.GetFromJsonAsync<List<Employee>>("/api/Usuarios");

        public Task AsignarUsbAsync(UsbAsignarRequestDto dto) =>
            _http.PostAsJsonAsync("/api/Usb/asignar", dto);

        public async Task<List<LogActividadDto>> GetLogsAsync(int page = 1, int pageSize = 50)
        {
            var res = await _http.GetFromJsonAsync<List<LogActividadDto>>(
                          $"/api/Logs?page={page}&pageSize={pageSize}");
            return res ?? new();
        }

        /* ══════════════════════════════════
           SECTION B · Funciones agente EMPLEADO (USB/PKI)
           ══════════════════════════════════ */

        // 1) Verificar certificado USB  → challenge Base64
        public async Task<string?> VerifyUsbAsync(string serial, string certPem, CancellationToken ct = default)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/auth/verify-usb",
                             new { Serial = serial, CertPem = certPem }, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    LogDebug($"[VerifyUsb] HTTP {(int)resp.StatusCode}");
                    return null;
                }
                return await resp.Content.ReadAsStringAsync(ct);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Detalle excepción", MessageBoxButtons.OK,MessageBoxIcon.Error);
                MostrarErrorConexion(ex);
                return null;
            }
        }

        public async Task<(bool ok, string? msg)> LoginAsync(
        string serial, string sig, string pin, string mac)
        {
            var json = new
            {
                serial,
                signatureBase64 = sig,
                pin,
                macAddress = mac
            };
            var resp = await _http.PostAsJsonAsync("api/auth/login", json);

            string body = await resp.Content.ReadAsStringAsync();
            if (resp.IsSuccessStatusCode)
                return (true, null);

            return (false, body.Length > 200 ? resp.StatusCode.ToString() : body);
        }

        // 2) Login con firma + PIN
        public async Task<(bool ok, string? msg)> LoginUsbAsync(
                string serial, string signatureB64, string pin, string mac, CancellationToken ct = default)
        {
            var payload = new
            {
                serial,
                signatureBase64 = signatureB64,
                pin,
                macAddress = mac
            };
            var resp = await _http.PostAsJsonAsync("api/auth/login", payload, ct);
            string body = await resp.Content.ReadAsStringAsync(ct);
            return resp.IsSuccessStatusCode
                   ? (true, null)
                   : (false, body.Length > 200 ? resp.StatusCode.ToString() : body);
        }

        // 3) Enviar lote JSON de eventos
        public async Task<bool> SendLogsAsync(IEnumerable<LogEvent> events, CancellationToken ct = default)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/logs", events, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    LogDebug($"[SendLogs] HTTP {(int)resp.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogDebug($"[SendLogs] ERROR {ex.Message}");
                return false;
            }
        }

        // 4) Enviar archivo .enc cifrado
        public async Task<bool> SendEncryptedLogAsync(string serial, string encPath, CancellationToken ct = default)
        {
            if (!File.Exists(encPath)) return false;
            try
            {
                using var content = new MultipartFormDataContent
                {
                    { new StreamContent(File.OpenRead(encPath)), "logfile", Path.GetFileName(encPath) },
                    { new StringContent(serial), "serial" }
                };
                var resp = await _http.PostAsync("api/logs/upload", content, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    LogDebug($"[SendEncrypted] HTTP {(int)resp.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogDebug($"[SendEncrypted] ERROR {ex.Message}");
                return false;
            }
        }

        /* ══════════════════════════════════
           SECTION C · Helpers
           ══════════════════════════════════ */

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
                MessageBox.Show($"Error de conexión:\n{ex.Message}",
                                "Error SSL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LogDebug($"[ApiClient] Conn ERROR {ex.Message}");
        }

        private static void LogDebug(string msg)
        {
            try
            {
                string dir = Path.Combine(Path.GetTempPath(), "RUSBP", "logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "debug.txt"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {msg}{Environment.NewLine}");
            }
            catch { /* ignore */ }
        }
        public async Task<HttpResponseMessage> SendAsync(HttpMethod method,
                                                 string path,
                                                 object? body = null,
                                                 CancellationToken ct = default)
        {
            using var msg = new HttpRequestMessage(method, path);
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body);
                msg.Content = new StringContent(json,
                                                Encoding.UTF8,
                                                "application/json");
            }

            var resp = await _http.SendAsync(msg, ct);
            resp.EnsureSuccessStatusCode();
            return resp;
        }

    }
}
