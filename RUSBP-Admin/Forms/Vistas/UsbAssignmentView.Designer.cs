using System;
using System.Drawing;
using System.Windows.Forms;

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

            // === Panel Izquierdo: USB ===
            panelLeft = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml("#0C1C36"),
                Padding = new Padding(24, 40, 24, 24),   // top = 40
                MinimumSize = new Size(260, 0)             // ancho fijo
            };

            lblUsbSection = new Label
            {
                Text = "Conexión USB",
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 14, 0, 18)
            };

            picUsb = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 120,
                Image = Properties.Resources.usb_on,
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
            btnDetect.FlatAppearance.BorderSize = 0;
            btnDetect.Click += btnDetect_Click;

            _lblUsbName = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 13, FontStyle.Regular),
                ForeColor = Color.White,
                Text = "Nombre del Dispositivo",
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 10, 0, 2)
            };

            _lblUsbSize = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Gainsboro,
                Text = "-",
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 10)
            };

            // Orden inverso: lo último es lo más arriba
            panelLeft.Controls.Add(_lblUsbSize);
            panelLeft.Controls.Add(_lblUsbName);
            panelLeft.Controls.Add(btnDetect);
            panelLeft.Controls.Add(picUsb);
            panelLeft.Controls.Add(lblUsbSection);

            // === Panel Central: Configuración USB ===
            _grpInfo = new GroupBox
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 24, 18, 24),
                BackColor = ColorTranslator.FromHtml("#121B26"),
                ForeColor = Color.White
            };

            lblMain = new Label
            {
                Text = "Configuración del Dispositivo USB",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 44,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSub = new Label
            {
                Text = "Información del Empleado",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 26,
                ForeColor = Color.White,
                Margin = new Padding(0, 12, 0, 4)
            };

            // Entradas de Usuario
            txtNombre = MakeInput("Nombre Completo");
            txtRut = MakeInput("RUT");
            txtDepto = MakeInput("Departamento");
            txtMail = MakeInput("Correo Electrónico");
            txtPin = MakeInput("PIN", true);

            // Rol
            lblRol = new Label
            {
                Text = "Rol",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 14, 0, 6)
            };
            cmbRol = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };
            cmbRol.Items.AddRange(new object[] { "Admin", "User" });
            cmbRol.SelectedIndex = 1;

            btnGenPki = new Button
            {
                Text = "Generar claves PKI",
                Width = 150,
                Height = 32,
                BackColor = Color.FromArgb(32, 45, 58),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnGenPki.FlatAppearance.BorderSize = 0;
            btnGenPki.Click += btnGenPki_Click;

            btnEncrypt = new Button
            {
                Text = "Cifrar Unidad",
                Width = 150,
                Height = 32,
                BackColor = Color.FromArgb(38, 97, 214),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(8, 10, 0, 4)
            };
            btnEncrypt.FlatAppearance.BorderSize = 0;
            btnEncrypt.Click += btnEncrypt_Click;

            progressBar = new ProgressBar
            {
                Width = 340,
                Height = 20,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Margin = new Padding(0, 4, 0, 10),
                Style = ProgressBarStyle.Continuous
            };

            chkCert = MakeCheck("Certificado Público");
            chkPriv = MakeCheck("Clave Privada Cifrada");
            chkCfg = MakeCheck("Archivos de Configuración");

            btnCreate = new Button
            {
                Text = "Crear Ambiente del USB",
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.FromArgb(26, 255, 176),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += btnCreate_Click;

            // TableLayout para ordenar los campos
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 16,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(8)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));

            int row = 0;
            tbl.Controls.Add(lblMain, 0, row);
            tbl.SetColumnSpan(lblMain, 2);

            tbl.Controls.Add(lblSub, 0, ++row);
            tbl.SetColumnSpan(lblSub, 2);

            tbl.Controls.Add(btnEncrypt, 1, ++row);

            tbl.Controls.Add(progressBar, 0, ++row);
            tbl.SetColumnSpan(progressBar, 2);

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

            // === Panel Derecho: Logs ===
            grpLogs = new GroupBox
            {
                Text = "Logs del Sistema",
                Dock = DockStyle.Fill,
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

            // === Layout responsivo ===
            var rootTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28)); // USB
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44)); // Central
            rootTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28)); // Logs

            rootTable.Controls.Add(panelLeft, 0, 0);
            rootTable.Controls.Add(_grpInfo, 1, 0);
            rootTable.Controls.Add(grpLogs, 2, 0);

            Controls.Add(rootTable);
            Size = new Size(1120, 720);

            // deshabilitar formulario hasta detectar USB
            ToggleForm(false);
        }

        // Helpers para UI
        private TextBox MakeInput(string placeholder, bool isPassword = false)
        {
            var tb = new TextBox
            {
                Width = 260,
                Font = new Font("Segoe UI", 12),
                PlaceholderText = placeholder,
                Margin = new Padding(0, 5, 0, 5)
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
                Margin = new Padding(0, 4, 0, 4),
                AutoSize = true,
                Checked = false,
                Enabled = false
            };
        }
        #endregion

        // Controles del formulario
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
        private Button btnEncrypt;
        private ProgressBar progressBar;

        private GroupBox grpLogs;
        private TextBox logsTextBox;
    }
}
