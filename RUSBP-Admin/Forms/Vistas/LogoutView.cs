using System;
using System.Drawing;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class LogoutView : UserControl
    {
        public event EventHandler? LogoutConfirmed;

        /* ──────────── controles ──────────── */
        private readonly PictureBox _picUsb;
        private readonly Button _btnSalir;

        public LogoutView()
        {
            DoubleBuffered = true;
            Dock = DockStyle.Fill;

            /* Fondo */
            BackgroundImage = Properties.Resources.Block_Screen_WALLPAPER;
            BackgroundImageLayout = ImageLayout.Stretch;

            /* Icono USB */
            _picUsb = new PictureBox
            {
                Image = Properties.Resources.usb_on, // cámbialo si tienes otro
                SizeMode = PictureBoxSizeMode.AutoSize,
            };
            Controls.Add(_picUsb);

            /* Botón salir */
            _btnSalir = new Button
            {
                Text = "SALIR",
                Size = new Size(260, 60),
                Font = new Font(FontFamily.GenericSansSerif, 18, FontStyle.Bold),
                BackColor = ColorTranslator.FromHtml("#0066A1"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnSalir.FlatAppearance.BorderSize = 0;
            _btnSalir.Click += (_, __) => LogoutConfirmed?.Invoke(this, EventArgs.Empty);
            Controls.Add(_btnSalir);

            /* Re-colocar al redimensionar */
            Resize += (_, __) => Reposition();
            Reposition();
        }

        /* ──────────── posición ──────────── */
        private void Reposition()
        {
            const double COLUMN_FACTOR = 0.33;          // ⅓ del ancho
            int columnX = (int)(ClientSize.Width * COLUMN_FACTOR);

            /* Icono */
            _picUsb.Location = new Point(
                columnX - _picUsb.Width / 2,
                ClientSize.Height / 2 - _picUsb.Height - 30);

            /* Botón */
            _btnSalir.Location = new Point(
                columnX - _btnSalir.Width / 2,
                _picUsb.Bottom + 20);
        }
    }
}
