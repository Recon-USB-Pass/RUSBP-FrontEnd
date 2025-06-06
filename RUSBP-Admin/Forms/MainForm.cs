using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Vistas;

namespace RUSBP_Admin.Forms
{
    public partial class MainForm : Form
    {
        private readonly MonitoringService _mon;
        private readonly ApiClient _api;
        private static NotifyIcon? _tray;

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

            // Inyectar servicios
            _monitoringView.SetServices(_mon, _api);
            _monitoringView.EmployeeSelected += emp =>
            {
                _detailView.LoadSingleEmployee(emp);
                ShowView(_detailView);
            };
            _assignmentView.UsbPrepared += _ => ShowView(_monitoringView);

            _logoutView.LogoutConfirmed += async (_, __) => await RealLogoutAsync();

            // Navegación
            _navBar.SectionSelected += tag => ShowSection(tag);
            ShowSection("Monitor");     // inicio, así también resalta la sección
        }

        private void ShowSection(string tag)
        {
            _navBar.SetActive(tag); // <-- Marca el icono activo

            switch (tag)
            {
                case "Monitor": ShowView(_monitoringView); break;
                case "Assign": ShowView(_assignmentView); break;
                case "Logout": ShowView(_logoutView); break;
                    // puedes agregar "Logs", "Profile" aquí si tienes esas vistas
            }
        }

        private async Task RealLogoutAsync()
        {
            try { await _api.SendAsync(HttpMethod.Post, "/auth/logout"); }
            catch { /* sin red, salimos igual */ }
            finally
            {
                _tray!.Visible = false;
                _tray.Dispose();
                Application.Restart();
            }
        }

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
