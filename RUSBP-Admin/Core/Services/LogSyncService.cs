using System.Text.Json;
using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Models.Dtos;

namespace RUSBP_Admin.Core.Services
{
    /// <summary>
    /// Se encarga de sincronizar los logs locales (uno por USB) con el backend via API.
    /// </summary>
    public class LogSyncService
    {
        private readonly ApiClient _api;
        private readonly string _logsDir;
        private readonly HashSet<string> _alreadySynced = new(); // Opcional: para no reenviar

        public LogSyncService(ApiClient api)
        {
            _api = api;
            _logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(_logsDir);
        }

        /// <summary>
        /// Sincroniza todos los archivos de log locales con el backend.
        /// </summary>
        public async Task SyncAllAsync(CancellationToken ct = default)
        {
            var files = Directory.GetFiles(_logsDir, "*.json");
            foreach (var file in files)
            {
                await SyncFileAsync(file, ct);
            }
        }

        /// <summary>
        /// Sincroniza el log de un USB específico (archivo .json, no cifrado).
        /// </summary>
        public async Task SyncFileAsync(string logJsonPath, CancellationToken ct = default)
        {
            List<LogEvent>? logs;
            try
            {
                var json = await File.ReadAllTextAsync(logJsonPath, ct);
                logs = JsonSerializer.Deserialize<List<LogEvent>>(json);
            }
            catch
            {
                return; // Si no se puede leer, ignora el archivo
            }

            if (logs == null || logs.Count == 0) return;

            // Filtra eventos nuevos no sincronizados aún
            var nuevos = logs.Where(ev => !_alreadySynced.Contains(ev.EventId)).ToList();
            if (nuevos.Count == 0) return;

            // Mapear a DTO
            var dtoList = nuevos.Select(ev => new LogEventDto(
                ev.EventId,
                ev.UserRut,
                ev.UsbSerial,
                ev.EventType,
                ev.Ip,
                ev.Mac,
                ev.Timestamp
            )).ToList();

            var ok = await _api.SendLogsAsync(dtoList, ct);
            if (ok)
            {
                foreach (var ev in nuevos)
                    _alreadySynced.Add(ev.EventId);
            }
        }

        /// <summary>
        /// Sincroniza solo el log de un serial USB.
        /// </summary>
        public async Task SyncUsbAsync(string serial, CancellationToken ct = default)
        {
            var path = Path.Combine(_logsDir, $"{serial.ToUpperInvariant()}.json");
            if (File.Exists(path))
                await SyncFileAsync(path, ct);
        }
    }
}
