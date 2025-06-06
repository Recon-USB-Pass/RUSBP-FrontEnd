using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Models.Dtos;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RUSBP_Admin.Core.Services
{
    

    public record RecoverUsbResponse(bool Ok, string? Err,
                                         string CipherB64, string TagB64);



    public class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(string backendIp)
        {
            backendIp = backendIp?.Trim() ?? "";

            // Si viene sin http, lo agrega
            string url = backendIp.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? backendIp
                : $"https://{backendIp}";

            // Si NO hay puerto explícito, agrega :8443
            var uriBuilder = new UriBuilder(url);
            if (uriBuilder.Port == 443 || uriBuilder.Port == 80) // default port => no puerto explícito
            {
                uriBuilder.Port = 8443;
            }

            // Forzar slash final
            if (!uriBuilder.Path.EndsWith("/"))
                uriBuilder.Path += "/";

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true // Solo para testing
            };

            try
            {
                _http = new HttpClient(handler)
                {
                    BaseAddress = uriBuilder.Uri,
                    Timeout = TimeSpan.FromSeconds(30)
                };
            }
            catch (UriFormatException ex)
            {
                MessageBox.Show($"URL inválida para el backend: '{uriBuilder.Uri}'\n{ex.Message}",
                    "Error URL Backend", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

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

        // ──────────────── C: Recuperación de RecoveryPassword (DESBLOQUEO USB ROOT ADMIN) ────────────────

        public async Task<RecoverUsbResponse> RecoverUsbAsync(string serial, int agentType)
        {
            var body = new { serial, agentType };
            var resp = await _http.PostAsJsonAsync("api/usb/recover", body);

            if (!resp.IsSuccessStatusCode)
            {
                string err = await resp.Content.ReadAsStringAsync();
                return new RecoverUsbResponse(false, err, "", "");
            }

            var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
            string cipherB64 = json!.GetProperty("cipher").GetString()!;
            string tagB64 = json.GetProperty("tag").GetString()!;
            return new RecoverUsbResponse(true, null, cipherB64, tagB64);
        }

        // ──────────────── C.1: Auth para agentes (challenge-response) ────────────────

        public async Task<string?> VerifyUsbAsync(string serial, string certPem, CancellationToken ct = default)
        {
            var dto = new { serial, certPem }; // El backend espera estas propiedades exactas
            string payload = System.Text.Json.JsonSerializer.Serialize(dto);
            Console.WriteLine($"[DEBUG-API] verify-usb payload: {payload}");

            var resp = await _http.PostAsJsonAsync(U("api/auth/verify-usb"), dto, ct);
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DEBUG-API] verify-usb resp: {resp.StatusCode}, content: {await resp.Content.ReadAsStringAsync()}");
                return null;
            }
            string content = await resp.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"[DEBUG-API] verify-usb response: {content}");
            return content;
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
            var json = JsonSerializer.Serialize(dto);
            System.Diagnostics.Debug.WriteLine("[DEBUG] Payload recover: " + json);

            var res = await RecoverAsync(dto);
            return res != null
                ? (res.Usuario, null)
                : (null, "PIN incorrecto o error de autenticación.");
        }

        public async Task<(bool ok, string? msg)> LoginAsync(string serial, string sig, string pin, string mac)
        {
            var json = new
            {
                serial,
                signatureBase64 = sig,
                pin,
                macAddress = mac
            };
            var resp = await _http.PostAsJsonAsync("api/auth/recover", json);
            string body = await resp.Content.ReadAsStringAsync();
            if (resp.IsSuccessStatusCode)
                return (true, null);

            return (false, body.Length > 200 ? resp.StatusCode.ToString() : body);
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
        // Envía acceso con ip/mac donde se logueó este USB
        public async Task PostAccesoAsync(string rut, string serialUsb, string ip, string mac, string pcName)
        {
            var body = new
            {
                rut,
                serialUsb,
                ip,
                mac,
                pcName
            };
            await _http.PostAsJsonAsync(U("api/Accesos"), body);
        }
        // En ApiClient.cs
        public async Task<(string ip, string mac, string pcName)> GetUltimoAccesoAsync(string serial)
        {
            try
            {
                var url = $"api/Accesos/ultimo?serial={serial}";
                var resp = await _http.GetAsync(U(url));
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Error {resp.StatusCode}: {json}", "ERROR API ACCESO");
                    return ("", "", "");
                }
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                return (
                    root.GetProperty("ip").GetString() ?? "",
                    root.GetProperty("mac").GetString() ?? "",
                    root.GetProperty("pcName").GetString() ?? ""
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception API acceso: {ex}", "API ERROR");
                return ("", "", "");
            }
        }



    }
}
