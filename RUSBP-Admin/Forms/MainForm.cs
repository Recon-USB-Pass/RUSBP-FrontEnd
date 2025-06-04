using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Vistas;

namespace RUSBP_Admin.Forms
{
    public partial class MainForm : Form
    {
        /* ─────────── Campos ─────────── */
        private readonly MonitoringService _mon;
        private readonly ApiClient _api;
        private static NotifyIcon? _tray;

        /* ─────────── Vistas ─────────── */
        private readonly MonitoringView _monitoringView = new();
        private readonly UsbCryptoService _usbService = new UsbCryptoService();
        private readonly UsbAssignmentView _assignmentView;
        private readonly EmployeeDetailView _detailView = new();
        private readonly LogoutView _logoutView = new();

        public MainForm(MonitoringService mon, ApiClient api)
        {
            _mon = mon;
            _api = api;

            InitializeComponent();

            _assignmentView = new UsbAssignmentView(api, _usbService);

            /* Tray-icon (restaurar) */
            if (_tray == null)
            {
                _tray = new NotifyIcon
                {
                    Icon = Icon = SystemIcons.Shield,
                    Text = "RUSBP – Consola admin",
                    Visible = true
                };
                _tray.DoubleClick += (_, __) =>
                {
                    WindowState = FormWindowState.Normal;
                    Activate();
                };
            }

            /* Inyectar servicios */
            _monitoringView.SetServices(_mon, _api);
            _monitoringView.EmployeeSelected += emp =>
            {
                _detailView.LoadSingleEmployee(emp);
                ShowView(_detailView);
            };
            _assignmentView.UsbPrepared += _ => ShowView(_monitoringView);

            /* LogoutView → evento Confirmar */
            _logoutView.LogoutConfirmed += async (_, __) => await RealLogoutAsync();

            /* Navegación */
            _navBar.SectionSelected += tag => ShowSection(tag);
            ShowView(_monitoringView);     // inicio
        }

        /* Navegación sin async */
        private void ShowSection(string tag)
        {
            switch (tag)
            {
                case "Monitor": ShowView(_monitoringView); break;
                case "Assign": ShowView(_assignmentView); break;
                case "Logout": ShowView(_logoutView); break;
            }
        }

        /* Logout definitivo (salida / restart) */
        private async Task RealLogoutAsync()
        {
            try { await _api.SendAsync(HttpMethod.Post, "/auth/logout"); }
            catch { /* sin red, salimos igual */ }
            finally
            {
                _tray!.Visible = false;
                _tray.Dispose();
                Application.Restart();          // vuelve al LoginForm
            }
        }

        /* Helper de vistas */
        private void ShowView(UserControl view)
        {
            _panelContent.SuspendLayout();
            _panelContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            _panelContent.Controls.Add(view);
            _panelContent.ResumeLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _tray!.Visible = false;
            _tray.Dispose();
            base.OnFormClosing(e);
        }
        public Task LogoutFromUsbRemovalAsync()
        {
            if (InvokeRequired)
            {
                BeginInvoke(async () => await RealLogoutAsync());
                return Task.CompletedTask;
            }
            return RealLogoutAsync();
        }
    }
}



