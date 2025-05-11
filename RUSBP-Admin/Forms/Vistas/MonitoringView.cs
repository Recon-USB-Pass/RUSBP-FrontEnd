using System;
using System.Drawing;
using System;
using System.Drawing;
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

        public delegate void EmployeeSelectedHandler(Employee emp);
        public event EmployeeSelectedHandler? EmployeeSelected;

        private const int PingTimeout = 2000;  // 2 s de timeout

        public MonitoringView() => InitializeComponent();

        public void SetServices(MonitoringService mon, ApiClient api)
        {
            _mon = mon;
            _api = api;
        }

        /* ===============================  Carga  ============================== */
        private async void MonitoringView_Load(object? sender, EventArgs e)
        {
            if (_api is null) return;

            var empleados = await _api.GetEmployeesAsync();
            flpCards.Controls.Clear();

            string ipAdmin = GetIpAddress.GetLocalIpAddress();
            string macAdmin = GetMacAddress.GetLocalMacAddress();

            foreach (var emp in empleados)
            {
                if (emp.Id == 1)           // ← admin / demo
                {
                    emp.Ip = ipAdmin;
                    emp.Mac = macAdmin;
                }

                string pingText = await GetPing(emp.Ip);
                var card = new UsbCardControl();
                card.LoadData(emp, pingText);
                card.CardClicked += OnUsbCardClick;
                flpCards.Controls.Add(card);
            }
        }

        private async Task<string> GetPing(string ip)
        {
            try
            {
                using var p = new Ping();
                var r = await p.SendPingAsync(ip, PingTimeout);
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