using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RUSBP_Admin.Core;
using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Shared;

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class MonitoringView : UserControl
    {
        private MonitoringService? _mon;
        private ApiClient? _api;
        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;

        private string _backendHost = new Uri(AppConfig.BackendBaseUrl).Host;


        public delegate void EmployeeSelectedHandler(Employee emp);
        public event EmployeeSelectedHandler? EmployeeSelected;

        private const int PingTimeout = 2000;          // 2 s

        public MonitoringView() => InitializeComponent();

        public void SetServices(MonitoringService mon, ApiClient api)
        {
            _mon = mon;
            _api = api;

            /* Intenta inferir host si ApiClient lo expone, si no usa el default. */
            _backendHost = TryGetHostFromApi(api) ?? _backendHost;
        }

        private static string? TryGetHostFromApi(ApiClient api)
        {
            /* reflection rápida, evita romper mientras no exista la propiedad */
            var prop = api.GetType().GetProperty("BaseAddress") ??
                       api.GetType().GetProperty("BaseUrl");

            if (prop?.GetValue(api) is Uri uri) return uri.Host;
            if (prop?.GetValue(api) is string s) return new Uri(s).Host;
            return null;
        }

        /* ---------------- ciclo de vida ---------------- */
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

        /* ---------------- construcción inicial ---------------- */
        private async Task LoadCardsAsync()
        {
            var empleados = await _api!.GetEmployeesAsync();

            flpCards.SuspendLayout();
            flpCards.Controls.Clear();

            foreach (var emp in empleados)
            {
                var card = new UsbCardControl();
                bool usbOk = false;                     // <<< sin propiedad aún
                // bool usbOk = emp.UsbConectado;       // descomenta cuando exista

                string ping = await MeasurePingAsync(_backendHost);

                card.LoadData(emp, usbOk, ping);
                card.CardClicked += OnUsbCardClick;

                flpCards.Controls.Add(card);
            }

            flpCards.ResumeLayout();
        }

        /* ---------------- refresco periódico ---------------- */
        private async Task RefreshLoopAsync(CancellationToken ct)
        {
            while (await _timer!.WaitForNextTickAsync(ct))
                await UpdateCardsAsync();
        }

        private async Task UpdateCardsAsync()
        {
            var cards = flpCards.Controls.OfType<UsbCardControl>().ToList();
            string pingCommon = await MeasurePingAsync(_backendHost);

            await Parallel.ForEachAsync(cards, async (card, _) =>
            {
                bool usbOk = false;
                // usbOk = await _api!.IsUsbOnlineAsync(card.Employee!.Id);  // cuando exista

                if (card.IsHandleCreated)
                    card.BeginInvoke(() => card.UpdatePing(pingCommon, usbOk));
            });
        }

        /* ---------------- utilidades ---------------- */
        private static async Task<string> MeasurePingAsync(string host)
        {
            try
            {
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









/*
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Shared;

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class MonitoringView : UserControl
    {
        private MonitoringService? _mon;
        private ApiClient? _api;

        public delegate void EmployeeSelectedHandler(Employee empleado);
        public event EmployeeSelectedHandler? EmployeeSelected;
        private const int PingTimeout = 2000; // 2 seconds timeout for ping request

        public MonitoringView()
        {
            InitializeComponent();
        }


        public void SetServices(MonitoringService mon, ApiClient api)
        {
            _mon = mon;
            _api = api;
        }

        private async void MonitoringView_Load(object sender, EventArgs e)
        {
            if (_api is null) return;

            var empleados = await _api.GetEmployeesAsync();
            flpCards.Controls.Clear();

            foreach (var emp in empleados)
            {
                var card = new UsbCardControl();
                string pingText = await GetPing(emp.Ip);
                card.LoadData(emp, pingText);
                card.Click += OnUsbCardClick;
                flpCards.Controls.Add(card);
            }
        }

        private async Task<string> GetPing(string ip)
        {
            try
            {
                using var p = new Ping();
                var reply = await p.SendPingAsync(ip, 500);
                return reply.Status == IPStatus.Success ? $"Ping: {reply.RoundtripTime}ms" : "Sin Conexión";
            }
            catch
            {
                return "Sin Conexión";
            }
        }

        private void OnUsbCardClick(object? sender, EventArgs e)
        {
            if (sender is UsbCardControl card && card.Employee is not null)
                EmployeeSelected?.Invoke(card.Employee);
        }
    }
}


*/