using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Models.Dtos;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class MonitoringView : UserControl
    {
        private MonitoringService? _mon;
        private ApiClient? _api;
        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;

        private const int PingTimeout = 2000;

        public delegate void EmployeeSelectedHandler(Employee emp);
        public event EmployeeSelectedHandler? EmployeeSelected;

        public MonitoringView() => InitializeComponent();

        public void SetServices(MonitoringService mon, ApiClient api)
        {
            _mon = mon;
            _api = api;
        }

        private async void MonitoringView_Load(object? sender, EventArgs e)
        {
            if (_api is null) return;

            await LoadCardsAsync();

            _cts = new();
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(AppSettings.PingIntervalMs));
            _ = RefreshLoopAsync(_cts.Token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }

        // --- Renderizado inicial ---
        private async Task LoadCardsAsync()
        {
            var usuarios = await _api!.GetUsuariosAsync();
            flpCards.SuspendLayout();
            flpCards.Controls.Clear();

            // DEBUG
            MessageBox.Show($"Cantidad usuarios: {usuarios.Count}\nPrimer serial: {usuarios.FirstOrDefault()?.Serial}", "DEBUG USUARIOS");

            foreach (var usuarioDto in usuarios)
            {
                // ==== 1. Consulta la última ubicación del USB ====
                string ip = "";
                string mac = "";
                string pcName = "";

                if (!string.IsNullOrEmpty(usuarioDto.Serial))
                {
                    try
                    {
                        // Llama al endpoint de accesos
                        var ultimo = await _api.GetUltimoAccesoAsync(usuarioDto.Serial);
                        ip = ultimo.ip;
                        mac = ultimo.mac;
                        pcName = ultimo.pcName;
                    }
                    catch (Exception ex)
                    {
                        // Si falla, deja campos vacíos y muestra un mensaje si es necesario.
                        //MessageBox.Show($"Error al consultar acceso de serial {usuarioDto.Serial}:\n{ex.Message}", "ERROR");
                    }
                }

                // ==== 2. Mapea a Employee ====
                var emp = new Employee
                {
                    Id = usuarioDto.Id,
                    Nombre = usuarioDto.Nombre,
                    Rut = usuarioDto.Rut,
                    Ip = ip,
                    Mac = mac,
                    Serial = usuarioDto.Serial,
                    Area = usuarioDto.Area,
                    Role = usuarioDto.Role,
                    StoragePercent = usuarioDto.StoragePercent,
                    PkiStatus = usuarioDto.PkiStatus
                };

                // Solo ping si hay IP, si no, no está conectado a ningún PC.
                string ping = string.IsNullOrEmpty(ip) ? "Sin Conexión" : await MeasurePingAsync(ip);
                bool usbOk = ping != "Sin Conexión";

                var card = new UsbCardControl();
                card.LoadData(emp, usbOk, ping);
                card.CardClicked += OnUsbCardClick;

                flpCards.Controls.Add(card);
            }

            flpCards.ResumeLayout();
        }

        // --- Refresco periódico de ping ---
        private async Task RefreshLoopAsync(CancellationToken ct)
        {
            while (await _timer!.WaitForNextTickAsync(ct))
                await UpdateCardsAsync();
        }

        private async Task UpdateCardsAsync()
        {
            var cards = flpCards.Controls.OfType<UsbCardControl>().ToList();

            await Parallel.ForEachAsync(cards, async (card, _) =>
            {
                var emp = card.Employee;
                if (emp == null) return;

                string pingTxt = string.IsNullOrEmpty(emp.Ip) ? "Sin Conexión" : await MeasurePingAsync(emp.Ip);
                bool usbOk = pingTxt != "Sin Conexión";

                if (card.IsHandleCreated)
                    card.BeginInvoke(() => card.UpdatePing(pingTxt, usbOk));
            });
        }

        // --- Utilidad: ping a IP de cada empleado ---
        private static async Task<string> MeasurePingAsync(string host)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(host)) return "Sin Conexión";
                using var p = new Ping();
                var r = await p.SendPingAsync(host, PingTimeout);
                return r.Status == IPStatus.Success
                     ? $"Ping: {r.RoundtripTime} ms"
                     : "Sin Conexión";
            }
            catch { return "Sin Conexión"; }
        }

        private void OnUsbCardClick(object? s, EventArgs e)
        {
            if (s is UsbCardControl c && c.Employee != null)
                EmployeeSelected?.Invoke(c.Employee);
        }
    }
}
