using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RUSBP_Admin.Core.Helpers;    // KeyboardHook, CursorGuard
using RUSBP_Admin.Core.Services;   // UsbWatcher, PinValidator

namespace RUSBP_Admin.Forms
{
    public partial class LoginForm : Form
    {
        /* CONFIG ----------------------------------------------------------- */
        private const bool LOCK_MODE = false;   // true = pantalla de bloqueo
        private const string VALID_PIN = "1234";
        private const int MAX_TRIES = 3;
        private const string IMG_DIR = "Images";
        private const string IMG_ON = "usb_icon_on.png";
        private const string IMG_OFF = "usb_icon_off.png";

        /* SERVICIOS --------------------------------------------------------- */
        private readonly UsbWatcher _usb;
        private readonly PinValidator _pin;

        public LoginForm()
        {
            InitializeComponent();

            /*--- aspecto ventana ---*/
            FormBorderStyle = LOCK_MODE ? FormBorderStyle.None : FormBorderStyle.Sizable;
            ControlBox = MaximizeBox = MinimizeBox = !LOCK_MODE;
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;

            /*--- hooks globales ---*/
            KeyboardHook.Install();

            /*--- restringe cursor sólo CUANDO el form ya está visible ---*/
            Shown += (_, __) =>
            {
                // margen de 80 px dentro del interior del LoginForm
                CursorGuard.RestrictToControl(this, new Padding(80));
            };

            /*--- servicios ---*/
            _usb = new UsbWatcher();
            _usb.StateChanged += ok => Invoke(() => UpdateUi(ok));

            _pin = new PinValidator(VALID_PIN, MAX_TRIES);
            _pin.MaxReached += () =>
            {
                MessageBox.Show("Máximo de intentos. Apagando.");
                Process.Start("shutdown", "/s /f /t 0");
            };

            UpdateUi(false);
            btnLogin.Click += btnLogin_Click;
        }

        /* UI ----------------------------------------------------------------*/
        private void UpdateUi(bool usbOk)
        {
            var img = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IMG_DIR,
                                   usbOk ? IMG_ON : IMG_OFF);

            if (File.Exists(img)) picUsb.Image = Image.FromFile(img);

            txtPin.Enabled = usbOk;
            btnLogin.Enabled = usbOk;
            if (!usbOk) txtPin.Clear();
        }

        private void btnLogin_Click(object? s, EventArgs e)
        {
            if (_pin.Check(txtPin.Text))
            {
                CursorGuard.Release();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("PIN incorrecto");
                txtPin.SelectAll(); txtPin.Focus();
            }
        }

        /* LIMPIEZA ----------------------------------------------------------*/
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _usb.Dispose();
            KeyboardHook.Uninstall();
            CursorGuard.Release();
            base.OnFormClosed(e);
        }

        /* bloque extra de teclas -------------------------------------------*/
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
#if DEBUG
            return base.ProcessCmdKey(ref msg, keyData);
#else
            Keys[] block = { Keys.Alt | Keys.F4, Keys.Alt, Keys.Tab, Keys.LWin, Keys.RWin };
            if (block.Any(k => k == keyData)) return true;
            return base.ProcessCmdKey(ref msg, keyData);
#endif
        }
    }
}
