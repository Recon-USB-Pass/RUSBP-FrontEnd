using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Models.Dtos;

namespace RUSBP_Admin.Core.Services
{
    public class MonitoringService
    {
        private readonly ApiClient _api;
        private readonly Dictionary<int, Employee> _cache = [];

        public event Action? DataUpdated;

        public MonitoringService(ApiClient api) => _api = api;

        public IReadOnlyCollection<Employee> Employees => _cache.Values;

        public async Task StartPollingAsync(CancellationToken ct, int intervalMs = 5000)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var user = await _api.GetUsuarioAsync(1); // demo
                    if (user != null) Upsert(user);
                    DataUpdated?.Invoke();
                }
                catch { /* log */ }

                await Task.Delay(intervalMs, ct);
            }
        }


        private void Upsert(UsuarioDto dto)
        {
            if (!_cache.TryGetValue(dto.Id, out var emp))
                emp = _cache[dto.Id] = new Employee { Id = dto.Id };

            emp.Nombre = dto.Nombre;
            emp.Rut = dto.Rut;
            if (dto.Usbs.Count > 0)
            {
                var usb = dto.Usbs[0];
                emp.Serial = usb.Serial;
            }
        }
    }
}

