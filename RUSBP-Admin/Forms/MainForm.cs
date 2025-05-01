using System;
using System.Windows.Forms;
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

        public MainForm()
        {
            InitializeComponent();       // ← definido en Designer

            _nav = new NavigationBar { Dock = DockStyle.Left };
            _content = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };

            Controls.Add(_content);
            Controls.Add(_nav);

            _nav.SectionSelected += OnSectionSelected;

            ShowView(_monitoringView);   // vista inicial
        }

        /* -------- Navegación -------- */
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
