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

        public delegate void EmployeeSelectedHandler(Employee empleado);
        public event EmployeeSelectedHandler? EmployeeSelected;

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
