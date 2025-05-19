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
        private readonly UsbAssignmentView _assignmentView = new();
        private readonly EmployeeDetailView _detailView = new();
        private readonly LogoutView _logoutView = new();

        public MainForm(MonitoringService mon, ApiClient api)
        {
            _mon = mon;
            _api = api;

            InitializeComponent();

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






/*

using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Vistas;

namespace RUSBP_Admin.Forms
{
    public partial class MainForm : Form
    {
        private readonly MonitoringService _mon;
        //private readonly AuthService _auth;
        private readonly ApiClient _api;
        private readonly NotifyIcon _trayIcon;

        //  vistas
        private readonly MonitoringView _monitoringView = new();
        private readonly UsbAssignmentView _assignmentView = new();
        private readonly EmployeeDetailView _detailView = new();
        private readonly LogoutView _logoutView = new();
        
        MonitoringView monitoringView;



        public MainForm(MonitoringService mon, ApiClient api)
        {
            _mon = mon;
            //_auth = auth;
            _api = api;

            InitializeComponent();

            /* ----- 2. Tray-icon minimal ----- *//*
            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Shield, // tu ico
                Text = "RUSBP – Consola admin",
                Visible = true
            };
            _trayIcon.DoubleClick += (_, __) =>
            {
                WindowState = FormWindowState.Normal;
                Activate();
            };

            _monitoringView.SetServices(_mon, _api);   // ← ping + datos
            _assignmentView.UsbPrepared += NavBar_SectionSelected;   // opcional


            // hook para navegar al detalle
            _monitoringView.EmployeeSelected += emp =>
            {
                _detailView.LoadSingleEmployee(emp);   // cargamos el empleado
                ShowView(_detailView);           // navegamos al detalle
            };

            //_navBar.SectionSelected += NavBar_SectionSelected;

            _navBar.SectionSelected += async tag => await NavBar_SectionSelectedAsync(tag);  // ⇦ async

            ShowView(_monitoringView);           // vista inicial
        }

        private void NavBar_SectionSelected(string tag)
        {
            switch (tag)
            {
                case "Monitor": ShowView(_monitoringView); break;
                case "Assign": ShowView(_assignmentView); break;
                case "Logout": ShowView(_logoutView); break;
                default: return;
            }
        }

        /* ---------- navegación ---------- *//*
        private async Task NavBar_SectionSelectedAsync(string tag)
        {
            switch (tag)
            {
                case "Monitor": ShowView(_monitoringView); break;
                case "Assign": ShowView(_assignmentView); break;
                case "Logout": await DoLogoutAsync(); break;
            }
        }
        /* ---------- util ---------- *//*
        public void ShowView(UserControl view)
        {
            _panelContent.SuspendLayout();
            _panelContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            _panelContent.Controls.Add(view);
            _panelContent.ResumeLayout();
        }

        /*
        public void ShowView(UserControl view)
        {
            _panelContent.SuspendLayout();
            _panelContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            _panelContent.Controls.Add(view);
            _panelContent.ResumeLayout();
        }

        */
/* ---------- logout ---------- *//*
private async Task DoLogoutAsync()
{
    try
    {
        await _api.PostAsync("/auth/logout", null);
    }
    catch { /* ignora fallos de red, salimos igual *//* }
    finally
    {
        _trayIcon.Visible = false;   // evita ícono zombie
        _trayIcon.Dispose();
        Application.Restart();       // regresa al LoginForm
    }
}




protected override void OnFormClosing(FormClosingEventArgs e)
{
    _trayIcon.Visible = false;
    _trayIcon.Dispose();
    base.OnFormClosing(e);
}
}

}

*/


//AAAAAAAAAAAAAAAAAAAAAAAAAAA ANTIGUO X2


/*

using System;
using System.IO;
using System.Windows.Forms;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Shared;
using RUSBP_Admin.Forms.Vistas;    // carpeta Views

namespace RUSBP_Admin.Forms
{
    public partial class MainForm : Form
    {
        private readonly NavigationBar _nav;
        private readonly Panel _content;

        // vistas:
        private readonly MonitoringView _monitoringView = new();
        private readonly UsbAssignmentView _assignmentView = new();
        private readonly MonitoringService _mon;
        private readonly AuthService _auth;
        private readonly ApiClient _api;

        public MainForm(MonitoringService mon, AuthService auth, ApiClient api)
        {
            _mon = mon;
            _auth = auth;
            _api = api;
            InitializeComponent();

            _nav = new NavigationBar { Dock = DockStyle.Left };
            _content = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };

            Controls.Add(_content);
            Controls.Add(_nav);

            _nav.SectionSelected += OnSectionSelected;

            ShowView(_monitoringView);   // vista inicial
        }

        /* -------- Navegación -------- *//*
        private void OnSectionSelected(string tag)
        {
            switch (tag)
            {
                case "Monitor": ShowView(_monitoringView); break;
                case "Assign": ShowView(_assignmentView); break;
                case "Logout": Close(); break;
            }
        }

        private void ShowView(UserControl v)
        {
            _content.Controls.Clear();
            v.Dock = DockStyle.Fill;
            _content.Controls.Add(v);
        }
    }
}

*/
