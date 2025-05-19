using System;
using System.Drawing;
using System.Windows.Forms;
using RUSBP_Admin.Core;
using RUSBP_Admin.Forms.Shared;

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class SettingsView : UserControl
    {
        private readonly NumericUpDown _nudPing;

        public SettingsView()
        {
            InitializeComponent();

            /* ---------- título ---------- */
            var lblTitle = new Label
            {
                Text = "Configuración General",
                Font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(lblTitle);

            /* ---------- control ping ---------- */
            var lblPing = new Label
            {
                Text = "Intervalo de ping (segundos):",
                Font = new Font(FontFamily.GenericSansSerif, 11),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 70)
            };
            Controls.Add(lblPing);

            _nudPing = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 120,
                Value = AppSettings.PingIntervalMs / 1000,
                Location = new Point(20, 50),
                Width = 80
            };
            _nudPing.ValueChanged += (_, __) =>
                AppSettings.PingIntervalMs = (int)_nudPing.Value * 1000;

            Controls.Add(_nudPing);

            /* Estético */
            BackColor = Color.FromArgb(15, 22, 30);   // mismo fondo oscuro
            Dock = DockStyle.Fill;
        }
    }
}
