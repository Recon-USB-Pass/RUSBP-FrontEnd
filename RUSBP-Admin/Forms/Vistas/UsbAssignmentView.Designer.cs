namespace RUSBP_Admin.Forms.Vistas
{
    partial class UsbAssignmentView
    {
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components is not null) components.Dispose();
            base.Dispose(disposing);
        }

        #region Designer generated code
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            /* ========== Panel Izquierdo: USB ========== */
            panelLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(12, 28, 54),
                Padding = new Padding(24)
            };

            lblUsbSection = new Label
            {
                Text = "Conexión USB",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 18, 0, 18)
            };

            picUsb = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 160,
                Image = Properties.Resources.usb_on,   // Cambia si tienes un recurso custom
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 20, 0, 20)
            };

            btnDetect = new Button
            {
                Text = "Detectar USB",
                Dock = DockStyle.Top,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(38, 97, 214),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                Margin = new Padding(0, 16, 0, 12)
            };
            btnDetect.Click += btnDetect_Click;

            _lblUsbName = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 13, FontStyle.Regular),
                ForeColor = Color.White,
                Text = "Nombre del Dispositivo",
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 10, 0, 2)
            };

            _lblUsbSize = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gainsboro,
                Text = "-",
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 10)
            };

            panelLeft.Controls.Add(_lblUsbSize);
            panelLeft.Controls.Add(_lblUsbName);
            panelLeft.Controls.Add(btnDetect);
            panelLeft.Controls.Add(picUsb);
            panelLeft.Controls.Add(lblUsbSection);

            /* ========== Panel Central: Configuración USB ========== */
            _grpInfo = new GroupBox
            {
                Text = "",
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                BackColor = Color.FromArgb(18, 27, 38),
                ForeColor = Color.White
            };

            lblMain = new Label
            {
                Text = "Configuración del Dispositivo USB",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 48,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            /* Subtítulo */
            lblSub = new Label
            {
                Text = "Información del Empleado",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.White,
                Margin = new Padding(0, 14, 0, 4)
            };

            /* Entradas de Usuario */
            txtNombre = MakeInput("Nombre Completo");
            txtRut = MakeInput("RUT");
            txtDepto = MakeInput("Departamento");
            txtMail = MakeInput("Correo Electrónico");
            txtPin = MakeInput("PIN", true);

            /* Rol */
            lblRol = new Label
            {
                Text = "Rol",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 18, 0, 6)
            };
            cmbRol = new ComboBox
            {
                Width = 190,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                Items = { "Admin", "User" },
                SelectedIndex = 1
            };

            /* Botón PKI */
            btnGenPki = new Button
            {
                Text = "Generar claves PKI",
                Width = 170,
                Height = 35,
                BackColor = Color.FromArgb(32, 45, 58),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 0, 0, 0)
            };
            btnGenPki.Click += btnGenPki_Click;

            /* Checkboxes e iconos archivos generados */
            chkCert = MakeCheck("Certificado Público");
            chkPriv = MakeCheck("Clave Privada Cifrada");
            chkCfg = MakeCheck("Archivos de Configuración");

            /* Botón Final */
            btnCreate = new Button
            {
                Text = "Crear Ambiente del USB",
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Color.FromArgb(26, 255, 176),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black
            };
            btnCreate.Click += btnCreate_Click;

            /* TableLayout para ordenar los campos */
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 11,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));

            int row = 0;
            tbl.Controls.Add(lblMain, 0, row);
            tbl.SetColumnSpan(lblMain, 2);

            tbl.Controls.Add(lblSub, 0, ++row);
            tbl.SetColumnSpan(lblSub, 2);

            tbl.Controls.Add(txtNombre, 0, ++row);
            tbl.SetColumnSpan(txtNombre, 2);

            tbl.Controls.Add(txtRut, 0, ++row);
            tbl.SetColumnSpan(txtRut, 2);

            tbl.Controls.Add(txtDepto, 0, ++row);
            tbl.SetColumnSpan(txtDepto, 2);

            tbl.Controls.Add(txtMail, 0, ++row);
            tbl.SetColumnSpan(txtMail, 2);

            tbl.Controls.Add(txtPin, 0, ++row);
            tbl.SetColumnSpan(txtPin, 2);

            tbl.Controls.Add(lblRol, 0, ++row);
            tbl.Controls.Add(cmbRol, 1, row);

            tbl.Controls.Add(btnGenPki, 1, ++row);

            tbl.Controls.Add(chkCert, 0, ++row);
            tbl.Controls.Add(chkPriv, 1, row);
            tbl.Controls.Add(chkCfg, 0, ++row);
            tbl.SetColumnSpan(chkCfg, 2);

            _grpInfo.Controls.Add(tbl);
            _grpInfo.Controls.Add(btnCreate);

            /* ========== Panel Derecho: Logs con TextBox ========== */
            grpLogs = new GroupBox
            {
                Text = "Logs del Sistema",
                Dock = DockStyle.Fill, // Para que crezca
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };
            logsTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = Color.White,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10),
                WordWrap = false
            };
            grpLogs.Controls.Add(logsTextBox);

            /* ========== Layout Responsivo: TableLayoutPanel ========== */
            var rootTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // USB
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Central
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Logs

            rootTable.Controls.Add(panelLeft, 0, 0);
            rootTable.Controls.Add(_grpInfo, 1, 0);
            rootTable.Controls.Add(grpLogs, 2, 0);

            Controls.Add(rootTable);
            Size = new Size(1152, 768);

            /* deshabilitar formulario hasta detectar USB */
            ToggleForm(false);
        }

        /* -------- Helpers para UI -------- */
        private TextBox MakeInput(string placeholder, bool isPassword = false)
        {
            var tb = new TextBox
            {
                Width = 320,
                Font = new Font("Segoe UI", 12),
                PlaceholderText = placeholder,
                Margin = new Padding(0, 6, 0, 6)
            };
            if (isPassword) tb.PasswordChar = '●';
            return tb;
        }

        private CheckBox MakeCheck(string text)
        {
            return new CheckBox
            {
                Text = text,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Margin = new Padding(0, 6, 0, 6),
                AutoSize = true,
                Checked = false,
                Enabled = false // sólo marcan el avance del proceso
            };
        }
        #endregion

        /* ------ Controles del formulario ------ */
        private System.ComponentModel.IContainer components = null!;
        private Panel panelLeft;
        private Label lblUsbSection;
        private PictureBox picUsb;
        private Button btnDetect;
        private Label _lblUsbName;
        private Label _lblUsbSize;

        private GroupBox _grpInfo;
        private Label lblMain;
        private Label lblSub;
        private TextBox txtNombre;
        private TextBox txtRut;
        private TextBox txtDepto;
        private TextBox txtMail;
        private TextBox txtPin;
        private Label lblRol;
        private ComboBox cmbRol;
        private Button btnGenPki;
        private Button btnCreate;
        private CheckBox chkCert, chkPriv, chkCfg;

        private GroupBox grpLogs;
        private TextBox logsTextBox;
    }
}
