using System;
using System.Windows.Forms;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Shared;
using RUSBP_Admin.Forms.Vistas;

namespace RUSBP_Admin.Forms
{
    public partial class MainForm : Form
    {
        private readonly MonitoringService _mon;
        private readonly AuthService _auth;
        private readonly ApiClient _api;

        //  vistas
        private readonly MonitoringView _monitoringView = new();
        private readonly UsbAssignmentView _assignmentView = new();
        private readonly EmployeeDetailView _detailView = new();
        private readonly LogoutView _logoutView = new();

        public MainForm(MonitoringService mon,
                        AuthService auth,
                        ApiClient api)
        {
            _mon = mon;
            _auth = auth;
            _api = api;

            InitializeComponent();

            // ↓ si quieres pasarles los servicios, hazlo por propiedades
            _monitoringView.SetServices(_mon, _api);
            _assignmentView.SetServices(_api);

            // hook para navegar al detalle


            _navBar.SectionSelected += NavBar_SectionSelected;

            ShowView(_monitoringView);           // vista inicial
        }

        private void NavBar_SectionSelected(string tag)
        {
            switch (tag)
            {
                case "Monitor": ShowView(_monitoringView); break;
                case "Assign": ShowView(_assignmentView); break;
                case "Logout": ShowView(_logoutView); break;
            }
        }

        private void ShowView(UserControl view)
        {
            _panelContent.SuspendLayout();
            _panelContent.Controls.Clear();
            _panelContent.Controls.Add(view);
            view.Dock = DockStyle.Fill;
            _panelContent.ResumeLayout();
        }
    }
}





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
