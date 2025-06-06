using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RUSBP_Admin.Core.Models;

namespace RUSBP_Admin.Forms.Shared
{
    public sealed partial class UsbCardControl : UserControl
    {
        public event EventHandler? CardClicked;
        public Employee? Employee { get; private set; }

        /* ---------- colores ---------- */
        private readonly Color _gradOk1 = ColorTranslator.FromHtml("#01864E");
        private readonly Color _gradOk2 = ColorTranslator.FromHtml("#0A536C");
        private readonly Color _gradFail = ColorTranslator.FromHtml("#444C58");

        /* ---------- controles ---------- */
        private readonly Label _lblName;
        private readonly Label _lblIp;
        private readonly Label _lblMac;
        private readonly Label _lblPing;

        private bool _usbOk;

        public UsbCardControl()
        {
            DoubleBuffered = true;
            Size = new Size(300, 180);
            Padding = new Padding(16);
            Cursor = Cursors.Hand;

            /* Click bubbling */
            Click += (_, __) => CardClicked?.Invoke(this, EventArgs.Empty);

            /* Labels */
            _lblName = MakeLabel("Nombre", 18, FontStyle.Bold);
            _lblIp = MakeLabel("", 11);
            _lblMac = MakeLabel("", 11);
            _lblPing = MakeLabel("", 11);

            /* Posiciones absolutas */
            _lblName.Location = new Point(100, 20);
            _lblIp.Location = new Point(100, 60);
            _lblMac.Location = new Point(100, 80);
            _lblPing.Location = new Point(100, 100);

            Controls.AddRange(new Control[] { _lblName, _lblIp, _lblMac, _lblPing });

            Paint += CardPaint;
        }

        /* ---------- API pública ---------- */
        public void LoadData(Employee emp, bool usbConnected, string pingTxt)
        {
            Employee = emp;
            _usbOk = usbConnected;
            _lblName.Text = emp.Nombre;
            _lblIp.Text = $"IP: {emp.Ip}";
            _lblMac.Text = $"MAC: {emp.Mac}";
            _lblPing.Text = pingTxt;
            Console.WriteLine($"USBCARDCONTROL: {emp.Nombre} IP:{emp.Ip} MAC:{emp.Mac} Ping:{pingTxt}");
            Invalidate();
        }


        public void UpdatePing(string pingTxt, bool usbConnected)
        {
            _lblPing.Text = pingTxt;
            _usbOk = usbConnected;
            Invalidate();
        }

        /* ---------- helpers ---------- */
        private static Label MakeLabel(string text, float size, FontStyle style = FontStyle.Regular)
            => new()
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font(FontFamily.GenericSansSerif, size, style),
                Text = text,
                BackColor = Color.Transparent
            };

        /* ---------- pintura ---------- */
        private void CardPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            /* fondo con esquinas redondeadas */
            using var path = Rounded(ClientRectangle, 12);
            using var brush = new LinearGradientBrush(
                                  ClientRectangle,
                                  _usbOk ? _gradOk1 : _gradFail,
                                  _usbOk ? _gradOk2 : _gradFail,
                                  90f);
            g.FillPath(brush, path);

            /* icono USB */
            var icon = _usbOk ? Properties.Resources.usb_on : Properties.Resources.usb_off;
            g.DrawImage(icon, 16, 16, 64, 64);
        }

        private static GraphicsPath Rounded(Rectangle rect, int radius)
        {
            var gp = new GraphicsPath();
            gp.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            gp.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            gp.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            gp.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}


