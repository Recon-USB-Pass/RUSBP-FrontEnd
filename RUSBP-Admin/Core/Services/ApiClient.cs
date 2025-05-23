// ApiClient.cs  –  v3 (lee appsettings + corrige rutas)
// ----------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Models.Dtos;

namespace RUSBP_Admin.Core.Services
{
    /// <summary>HttpClient único para todos los end-points (empleado + admin).</summary>
    public class ApiClient
    {
        private readonly HttpClient _http;

        /* ══════════════════ ctor ══════════════════ */
        public ApiClient(string baseUrl)
        {
            // Acepta cualquier certificado --- SÓLO para desarrollo
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };

            // Siempre garantizamos "/" al final
            if (!baseUrl.EndsWith('/')) baseUrl += "/";

            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        // ctor alternativo para DI / tests
        public ApiClient(HttpClient http) => _http = http;

        /* ════════════════════════════════════════════════════════
           Helper: construye la URI asegurando “/” y sin duplicar
           ════════════════════════════════════════════════════════ */
        private string U(string relative) =>
            relative.StartsWith('/') ? relative : "/" + relative;

        /* ════════════════════════════════════════════════════════
           SECTION A · End-points de administración
           ════════════════════════════════════════════════════════ */

        public Task LoginUserAsync(int usuarioId, string pin, string mac) =>
            _http.PostAsJsonAsync(U("api/auth/login"),
                new LoginRequestDto(usuarioId, pin, mac));

        public Task<UsuarioDto?> GetUsuarioAsync(int id) =>
            _http.GetFromJsonAsync<UsuarioDto>(U($"api/Usuarios/{id}"));

        public Task<List<Employee>?> GetEmployeesAsync() =>
            _http.GetFromJsonAsync<List<Employee>>(U("api/Usuarios"));

        public Task AsignarUsbAsync(UsbAsignarRequestDto dto) =>
            _http.PostAsJsonAsync(U("api/usb/asignar"), dto);

        public async Task<List<LogActividadDto>> GetLogsAsync(int page = 1, int pageSize = 50)
        {
            var res = await _http.GetFromJsonAsync<List<LogActividadDto>>(
                          U($"api/logs?page={page}&pageSize={pageSize}"));
            return res ?? new();
        }

        /* Post genérico (T) */
        public async Task<T> PostAsync<T>(string url, object body)
        {
            var resp = await _http.PostAsJsonAsync(U(url), body);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json)!;
        }

        public Task PostAsync(string url, object body) =>
            _http.PostAsJsonAsync(U(url), body);

        /* ════════════════════════════════════════════════════════
           SECTION B · Funciones del agente USB (empleado)
           ════════════════════════════════════════════════════════ */

        /// <summary>1) Verificar certificado: devuelve el “challenge” en Base64 o null.</summary>
        public async Task<string?> VerifyUsbAsync(string serial, string certPem,
                                                  CancellationToken ct = default)
        {
            try
            {
                var dto = new { serial, certPem };
                var resp = await _http.PostAsJsonAsync(U("api/auth/verify-usb"), dto, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    LogDebug($"[VerifyUsb] HTTP {(int)resp.StatusCode}");
                    return null;
                }
                return await resp.Content.ReadAsStringAsync(ct);
            }
            catch (Exception ex)
            {
                MostrarErrorConexion(ex);
                return null;
            }
        }

        /// <summary>2) Login con firma + PIN.</summary>
        public async Task<(UsuarioDto? usuario, string? error)> LoginUsbAsync(string serial, string signatureB64, string pin, string mac, CancellationToken ct = default)
        {
            var payload = new
            {
                serial,
                signatureBase64 = signatureB64,
                pin,
                macAddress = mac
            };

            var resp = await _http.PostAsJsonAsync(U("api/auth/login"), payload, ct);

            if (resp.IsSuccessStatusCode)
            {
                // Aquí deserializa el UsuarioDto directamente desde el JSON
                var usuario = await resp.Content.ReadFromJsonAsync<UsuarioDto>(cancellationToken: ct);
                return (usuario, null);
            }
            else
            {
                string body = await resp.Content.ReadAsStringAsync(ct);
                return (null, body.Length > 200 ? resp.StatusCode.ToString() : body);
            }
        }


        /// <summary>3) Sincronizar lote JSON de eventos.</summary>
        public async Task<bool> SendLogsAsync(IEnumerable<LogEvent> events,
                                              CancellationToken ct = default)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync(U("api/logs"), events, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    LogDebug($"[SendLogs] HTTP {(int)resp.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogDebug($"[SendLogs] {ex.Message}");
                return false;
            }
        }

        /// <summary>4) Subida de archivo de log cifrado (.enc)</summary>
        public async Task<bool> SendEncryptedLogAsync(string serial, string encPath, CancellationToken ct = default)
        {
            if (!File.Exists(encPath)) return false;

            FileStream? fileStream = null;
            StreamContent? fileContent = null;
            MultipartFormDataContent? content = null;

            try
            {
                fileStream = File.Open(encPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                fileContent = new StreamContent(fileStream);
                content = new MultipartFormDataContent
        {
            { fileContent, "logfile", Path.GetFileName(encPath) },
            { new StringContent(serial), "serial" }
        };

                var resp = await _http.PostAsync(U("api/logs/upload"), content, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    LogDebug($"[SendEncrypted] HTTP {(int)resp.StatusCode}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogDebug($"[SendEncrypted] {ex.Message}");
                return false;
            }
            finally
            {
                // El orden importa: primero el MultipartFormDataContent, luego los streams individuales
                content?.Dispose();
                fileContent?.Dispose();
                fileStream?.Dispose();
            }
        }


        /* ════════════════════════════════════════════════════════
           SECTION C · Utilidades de depuración
           ════════════════════════════════════════════════════════ */

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

        private static void LogDebug(string msg)
        {
            try
            {
                string dir = Path.Combine(Path.GetTempPath(), "RUSBP", "logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "debug.txt"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {msg}{Environment.NewLine}");
            }
            catch { /* swallow */ }
        }

        /* ───────────── Helper raw Send (poco usado) ───────────── */
        public async Task<HttpResponseMessage> SendAsync(HttpMethod method,
                                                         string path,
                                                         object? body = null,
                                                         CancellationToken ct = default)
        {
            using var msg = new HttpRequestMessage(method, U(path));
            if (body != null)
            {
                msg.Content = new StringContent(JsonSerializer.Serialize(body),
                                                Encoding.UTF8,
                                                "application/json");
            }

            var resp = await _http.SendAsync(msg, ct);
            resp.EnsureSuccessStatusCode();
            return resp;
        }
    }
}







