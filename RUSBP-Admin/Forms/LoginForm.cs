﻿using System.Net.NetworkInformation;
using RUSBP_Admin.Core.Helpers;
using RUSBP_Admin.Core.Services;

using System.Runtime.InteropServices;

namespace RUSBP_Admin
{
    /// <summary>Pantalla de bloqueo del empleado.</summary>
    public partial class LoginForm : Form
    {
        /* ───────────── Config ───────────── */
        private const bool FULLSCREEN = false;   // ← true para modo kiosco

        /* ───────────── Dependencias ───────────── */
        private readonly ApiClient _api;
        private readonly UsbCryptoService _usb;
        private readonly LogManager? _logManager;
        private readonly LogSyncService? _logSync;
        private readonly UsbWatcher _watcher;

        /* ───────────── Estado ───────────── */
        private string? _challenge;
        private string? _serial;
        private string? _userRut; // Asignado después del login si lo tienes

        /* ───────────── Reintento Verifiación ───────────── */
        private System.Windows.Forms.Timer? _retryTimer;
        private int _retryIntervalMs = 20000; // 20 segundos



        /* ───────────── LogOut ───────────── */
        private NotifyIcon? _trayIcon;
        private Button? _btnLogout;
        private bool _authSuccess = false;// ① campo nuevo


        /* ───────────── UI ───────────── */
        private PictureBox _picUsbOn = null!;
        private PictureBox _picUsbOff = null!;
        private TextBox _txtPin = null!;
        private Button _btnLogin = null!;
        private Label _lblStatus = null!;

        public LoginForm(ApiClient api, UsbCryptoService usb, LogManager? logManager = null, LogSyncService? logSync = null)
        {
            _api = api;
            _usb = usb;
            _logManager = logManager;
            _logSync = logSync;

            BuildUi();
            Resize += (_, __) => CenterControls();

            /* Bloqueo de teclas y cursor */
            KeyboardHook.Install();
            CursorGuard.RestrictToControl(this, new Padding(80));

            /* Watcher USB */
            _watcher = new UsbWatcher();
            _watcher.StateChanged += st =>
            {
                if (IsHandleCreated)
                    BeginInvoke(() => OnUsbStatusChanged(st));
            };
        }

        /* ───────────── UI helpers ───────────── */

        private void BuildUi()
        {
            Text = "RUSBP";
            if (FULLSCREEN)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                TopMost = true;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                StartPosition = FormStartPosition.CenterScreen;
                Size = new Size(900, 600);
            }

            BackgroundImage = Properties.Resources.Block_Screen_WALLPAPER;
            BackgroundImageLayout = ImageLayout.Stretch;

            /* Iconos USB */
            _picUsbOff = new PictureBox
            {
                Image = Properties.Resources.usb_off,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(140, 140)
            };
            _picUsbOn = new PictureBox
            {
                Image = Properties.Resources.usb_on,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = _picUsbOff.Size,
                Visible = false
            };

            /* Label estado */
            _lblStatus = new Label
            {
                AutoSize = true,
                Text = "Sin USB",
                ForeColor = Color.LightGray,
                Font = new Font(Font.FontFamily, 12, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            /* PIN */
            _txtPin = new TextBox
            {
                Width = 240,
                PlaceholderText = "Contraseña",
                PasswordChar = '●',
                Enabled = false,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font(Font.FontFamily, 14)
            };
            _txtPin.KeyPress += (_, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                    e.Handled = true;
            };

            /* Botón */
            _btnLogin = new Button
            {
                Text = "Entrar",
                Width = 240,
                Height = 45,
                Enabled = false,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = ColorTranslator.FromHtml("#0066A1"),
                Font = new Font(Font.FontFamily, 14, FontStyle.Bold)
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += async (_, __) => await OnLoginAsync();

            Controls.AddRange(new Control[]
            {
                _picUsbOff, _picUsbOn, _lblStatus, _txtPin, _btnLogin
            });

            CenterControls();
        }

        private void CenterControls()
        {
            const int VIRTUAL_WIDTH = 380;                         // ancho de la “columna”
            int panelX = (ClientSize.Width / 3) - (VIRTUAL_WIDTH / 2);
            int centerY = ClientSize.Height / 2;

            // USB icon
            int iconLeft = panelX + (VIRTUAL_WIDTH - _picUsbOff.Width) / 2;
            int iconTop = centerY - 200;
            _picUsbOff.Location = new Point(iconLeft, iconTop);
            _picUsbOn.Location = _picUsbOff.Location;

            // Label justo encima
            _lblStatus.Location = new Point(
                iconLeft + (_picUsbOff.Width - _lblStatus.Width) / 2,
                iconTop - 28);

            // PIN textbox
            _txtPin.Location = new Point(
                panelX + (VIRTUAL_WIDTH - _txtPin.Width) / 2,
                _picUsbOff.Bottom + 25);

            // Botón
            _btnLogin.Location = new Point(_txtPin.Left, _txtPin.Bottom + 15);
        }

        /* ───────────── Watcher callback ───────────── */

        private void OnUsbStatusChanged(UsbWatcher.UsbStatus st)
        {
            switch (st)
            {
                case UsbWatcher.UsbStatus.None:
                    // (1) Registro de desconexión forzosa
                    if (_serial != null && _logManager != null)
                    {
                        var log = new LogEvent
                        {
                            UserRut = _userRut ?? "(desconocido)",
                            UsbSerial = _serial,
                            EventType = "desconexión_forzada",
                            Ip = ObtenerIpLocal(),
                            Mac = "", // último MAC conocido si lo tienes
                            Timestamp = DateTime.UtcNow
                        };
                        _logManager.AddEvent(log);
                    }

                    // (2) Sincroniza log al desconectar
                    if (_logSync != null && _serial != null)
                        _ = _logSync.SyncUsbAsync(_serial);

                    // (3) Restaura la pantalla de login, aunque la ventana esté oculta o minimizada
                    Invoke(() =>
                    {
                        Show();
                        WindowState = FormWindowState.Normal;
                        ShowInTaskbar = true;
                        BringToFront();

                        // Reinicia la UI
                        _txtPin.Visible = true;
                        _btnLogin.Visible = true;
                        _btnLogout?.Hide();
                        _lblStatus.Text = "Sin USB";
                        _lblStatus.ForeColor = Color.LightGray;
                        _picUsbOff.Visible = true;
                        _picUsbOn.Visible = false;

                        // (opcional) Limpia campo PIN
                        _txtPin.Clear();
                        _txtPin.Focus();
                    });
                    break;

                case UsbWatcher.UsbStatus.Detected:
                    UpdateVisual(true);
                    _lblStatus.Text = "USB detectado";
                    _lblStatus.ForeColor = Color.DeepSkyBlue;
                    _ = BeginVerificationAsync();
                    break;

                case UsbWatcher.UsbStatus.Verified:
                    _lblStatus.Text = "Verificado";
                    _lblStatus.ForeColor = Color.LimeGreen;
                    break;

                case UsbWatcher.UsbStatus.Error:
                    _lblStatus.Text = "Error de verificación";
                    _lblStatus.ForeColor = Color.OrangeRed;
                    break;
            }
        }


        private void UpdateVisual(bool usbOk)
        {
            _picUsbOn.Visible = usbOk;
            _picUsbOff.Visible = !usbOk;
            _txtPin.Enabled = usbOk;
            _btnLogin.Enabled = usbOk;
            if (!usbOk) _txtPin.Clear();
            if (_btnLogout != null) _btnLogout.Visible = false;
        }


        /* ───────────── Verificación backend ───────────── */

        private async Task BeginVerificationAsync()
        {
            // 1) volver a localizar el USB (por si cambió la letra)
            if (!_usb.TryLocateUsb())
            {
                _watcher.SetVerified(false);
                return;
            }

            _serial = _usb.Serial;                     // viene en MAYÚSCULAS
            string certPem = _usb.LoadCertPem();       // lee ...\pki\cert.crt  (PEM)

            /* -------- 2) llamar al backend -------- */
            _lblStatus.Text = "Verificando…";
            _lblStatus.ForeColor = Color.DeepSkyBlue;

            try
            {
                _challenge = await _api.VerifyUsbAsync(_serial!, certPem);
            }
            catch (HttpRequestException)
            {
                _challenge = null;                     // forzamos re-intento más abajo
            }

            /* -------- 3) procesar resultado -------- */
            if (string.IsNullOrEmpty(_challenge))
            {
                _lblStatus.Text = "Sin conexión al servidor. Reintentando…";
                _lblStatus.ForeColor = Color.OrangeRed;
                ProgramarReintentoVerificacion();
                _watcher.SetVerified(false);
                return;
            }

            _lblStatus.Text = "USB verificado";
            _lblStatus.ForeColor = Color.LimeGreen;
            _watcher.SetVerified(true);
            DetenerReintentoVerificacion();
        }


        /* ───────────── Reintento de Varificacion con el Servidor ───────────── */

        private void ProgramarReintentoVerificacion()
        {
            if (_retryTimer == null)
            {
                _retryTimer = new System.Windows.Forms.Timer();
                _retryTimer.Interval = _retryIntervalMs;
                _retryTimer.Tick += async (s, e) =>
                {
                    _retryTimer!.Stop();
                    await BeginVerificationAsync();
                };
            }
            if (!_retryTimer.Enabled)
                _retryTimer.Start();
        }

        private void DetenerReintentoVerificacion()
        {
            if (_retryTimer != null && _retryTimer.Enabled)
                _retryTimer.Stop();
        }


        /* ───────────── Login (PIN) ───────────── */

        // Campo nuevo a nivel de clase (déjalo arriba, fuera del método):
        // private bool _authSuccess = false;



        private async Task OnLoginAsync()
        {
            if (_serial is null || _challenge is null) return;

            _btnLogin.Enabled = false;

            try
            {
                string sig = _usb.Sign(_challenge);
                string mac = NetworkInterface.GetAllNetworkInterfaces()
                                             .FirstOrDefault(i => i.OperationalStatus == OperationalStatus.Up)?
                                             .GetPhysicalAddress().ToString() ?? "";

                var (usuario, err) = await _api.LoginUsbAsync(_serial, sig, _txtPin.Text.Trim(), mac);

                if (usuario != null)
                {
                    // ⚡️ AQUÍ VALIDAS EL ROL PARA AGENTE ADMIN
                    if (usuario.Rol == null || !usuario.Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Acceso denegado: solo usuarios administradores pueden acceder a esta consola.",
                                        "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _btnLogin.Enabled = true;
                        _txtPin.Clear();
                        _txtPin.Focus();
                        return;
                    }

                    // REGISTRA el RUT global para logs, si lo usas
                    _userRut = usuario.Rut;

                    // 1. Registrar evento
                    _logManager?.AddEvent(new LogEvent
                    {
                        UserRut = _userRut ?? "(desconocido)",
                        UsbSerial = _serial,
                        EventType = "conexión",
                        Ip = ObtenerIpLocal(),
                        Mac = mac,
                        Timestamp = DateTime.UtcNow
                    });

                    // 2. Sincronizar logs pendientes
                    if (_logSync != null)
                        await _logSync.SyncUsbAsync(_serial);

                    // 3. Salida exitosa
                    CursorGuard.Release();
                    KeyboardHook.Uninstall();

                    _authSuccess = true;
                    _watcher.Dispose();
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                // --- Errores de challenge o backend ---
                if (err?.Contains("Challenge vencido") == true)
                {
                    MessageBox.Show("Challenge vencido. Reintentando...");
                }
                else
                {
                    MessageBox.Show(err ?? "Error desconocido");
                }
                await BeginVerificationAsync(); // re-verifica/rehace el reto
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión:\n" + ex.Message);

                _logManager?.AddEvent(new LogEvent
                {
                    UserRut = _userRut ?? "(desconocido)",
                    UsbSerial = _serial ?? "(desconocido)",
                    EventType = "login_fail",
                    Ip = ObtenerIpLocal(),
                    Timestamp = DateTime.UtcNow
                });
            }
            finally
            {
                _btnLogin.Enabled = true;
                _txtPin.SelectAll();
                _txtPin.Focus();
            }
        }


        /// <summary>Supervisa la presencia física del USB administrador.</summary>
        public sealed class UsbSessionGuard : IDisposable
        {
            private readonly UsbWatcher _watcher;
            private readonly Func<Task> _onUsbRemovedAsync;

            public UsbSessionGuard(Func<Task> onUsbRemovedAsync)
            {
                _onUsbRemovedAsync = onUsbRemovedAsync;
                _watcher = new UsbWatcher();
                _watcher.StateChanged += Handle;
            }

            private async void Handle(UsbWatcher.UsbStatus st)
            {
                if (st == UsbWatcher.UsbStatus.None)
                {
                    await _onUsbRemovedAsync();   // ① realiza logout / flush / restart
                    LockWorkStation();            // ② bloquea la estación
                }
            }

            [DllImport("user32.dll")] private static extern bool LockWorkStation();
            public void Dispose() => _watcher.Dispose();
        }

        // Muestra ventana desde tray icon
        private void MostrarVentana()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            BringToFront();
        }

        private void RealizarLogout()
        {
            // 1. Registrar evento logout si lo deseas
            if (_logManager != null && _serial != null)
            {
                var log = new LogEvent
                {
                    UserRut = _userRut ?? "(desconocido)",
                    UsbSerial = _serial,
                    EventType = "logout",
                    Ip = ObtenerIpLocal(),
                    Mac = "", // Puedes guardar el último MAC si quieres
                    Timestamp = DateTime.UtcNow
                };
                _logManager.AddEvent(log);
            }
            // 2. Sincronizar logs
            if (_logSync != null && _serial != null)
                _ = _logSync.SyncUsbAsync(_serial);

            // 3. Ejecutar LockWorkStation
            LockWorkStation();

            // 4. Restaurar UI para próximo login (opcional, si deseas reusar la ventana)
            Application.Exit(); // O puedes limpiar y mostrar la pantalla de login de nuevo
        }



        /* ───────────── Extras para loginAsync ───────────── */
        private string ObtenerIpLocal()
        {
            try
            {
                return System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                    .ToString() ?? "";
            }
            catch { return ""; }
        }

        private string ObtenerRutEmpleado()
        {
            // TODO: Implementa cómo obtener el RUT del usuario autenticado, por ejemplo desde USB o configuración.
            return _userRut ?? "(desconocido)";
        }

        /* ───────────── Limpieza ───────────── */
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_authSuccess)   // solo bloquea si NO fue login válido
                LockWorkStation();

            base.OnFormClosing(e);
        }

        /* Bloqueo extra de combinaciones */
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData is (Keys.Alt | Keys.F4) or Keys.Alt or Keys.Tab or Keys.LWin or Keys.RWin)
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
