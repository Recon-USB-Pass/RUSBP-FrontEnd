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

            /* ========== panel izquierdo (USB) ========== */
            panelLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.FromArgb(10, 25, 60),
                Padding = new Padding(20)
            };

            picUsb = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 180,
                Image = Properties.Resources.usb_icon_off,   // cambia si tienes otro recurso
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnDetect = new Button
            {
                Text = "Detectar USB",
                Dock = DockStyle.Top,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                Margin = new Padding(0, 20, 0, 10)
            };
            btnDetect.Click += btnDetect_Click;

            _lblUsbName = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = Color.White,
                Text = "Nombre del Dispositivo",
            };
            _lblUsbSize = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = Color.Gainsboro,
                Text = "-",
                Margin = new Padding(0, 4, 0, 0)
            };

            /* se añaden de abajo hacia arriba para que el orden visual sea correcto */
            panelLeft.Controls.Add(_lblUsbSize);
            panelLeft.Controls.Add(_lblUsbName);
            panelLeft.Controls.Add(btnDetect);
            panelLeft.Controls.Add(picUsb);

            /* ========== panel central (grupo info) ========== */
            _grpInfo = new GroupBox
            {
                Text = "Configuración del Dispositivo USB",
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ForeColor = Color.White
            };

            var lblNombre = new Label { Text = "Nombre completo", ForeColor = Color.White };
            var lblRut = new Label { Text = "RUT", ForeColor = Color.White };
            var lblDepto = new Label { Text = "Departamento", ForeColor = Color.White };
            var lblMail = new Label { Text = "Correo", ForeColor = Color.White };
            var lblRol = new Label { Text = "Rol", ForeColor = Color.White };

            txtNombre = new TextBox { Width = 260 };
            txtRut = new TextBox { Width = 260 };
            txtDepto = new TextBox { Width = 260 };
            txtMail = new TextBox { Width = 260 };
            cmbRol = new ComboBox
            {
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Admin", "User" }
            };

            btnGenPki = new Button
            {
                Text = "Generar claves PKI",
                Width = 130,
                Height = 28,
                BackColor = Color.DimGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenPki.Click += btnGenPki_Click;

            /* iconos de avance */
            _icoCert = MakeFolderIcon();
            _icoPriv = MakeFolderIcon();
            _icoCfg = MakeFolderIcon();

            /* botón final */
            btnCreate = new Button
            {
                Text = "Crear Ambiente del USB",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.MediumSpringGreen,
                FlatStyle = FlatStyle.Flat
            };
            btnCreate.Click += btnCreate_Click;

            /* diseño manual simple con TableLayout */
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 9,
                AutoSize = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            AddRow(table, 0, lblNombre, txtNombre);
            AddRow(table, 1, lblRut, txtRut);
            AddRow(table, 2, lblDepto, txtDepto);
            AddRow(table, 3, lblMail, txtMail);
            /* rol + botón PKI */
            table.Controls.Add(lblRol, 0, 4);
            table.Controls.Add(cmbRol, 1, 4);
            table.Controls.Add(btnGenPki, 2, 4);
            /* fila de iconos */
            table.Controls.Add(_icoCert, 0, 5);
            table.Controls.Add(new Label { Text = "Certificado público", ForeColor = Color.White }, 1, 5);
            table.Controls.Add(_icoPriv, 0, 6);
            table.Controls.Add(new Label { Text = "Clave privada cifrada", ForeColor = Color.White }, 1, 6);
            table.Controls.Add(_icoCfg, 0, 7);
            table.Controls.Add(new Label { Text = "Archivos de configuración", ForeColor = Color.White }, 1, 7);

            _grpInfo.Controls.Add(table);
            _grpInfo.Controls.Add(btnCreate);

            /* ========== panel derecho (logs) ========== */
            var grpLogs = new GroupBox
            {
                Text = "Logs del Sistema",
                Dock = DockStyle.Right,
                Width = 260,
                ForeColor = Color.White
            };
            lstLog = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = Color.White
            };
            grpLogs.Controls.Add(lstLog);

            /* ========== UserControl root ========== */
            BackColor = Color.FromArgb(16, 24, 32);
            Controls.Add(_grpInfo);
            Controls.Add(grpLogs);
            Controls.Add(panelLeft);
            Size = new Size(1152, 768);

            /* deshabilitar formulario hasta detectar USB */
            ToggleForm(false);

            /* helper local */
            PictureBox MakeFolderIcon() => new PictureBox
            {
                Image = Properties.Resources.usb_off,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Width = 32,
                Height = 32,
                Margin = new Padding(0, 4, 0, 4),
                BackColor = Color.DimGray
            };
        }

        private static void AddRow(TableLayoutPanel tbl, int row, Control lbl, Control ctr)
        {
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tbl.Controls.Add(lbl, 0, row);
            tbl.Controls.Add(ctr, 1, row);
        }
        #endregion

        private System.ComponentModel.IContainer components = null!;
        private Panel panelLeft;
        private PictureBox picUsb;
        private Button btnDetect;
        private Label _lblUsbName;
        private Label _lblUsbSize;

        private GroupBox _grpInfo;
        private TextBox txtNombre;
        private TextBox txtRut;
        private TextBox txtDepto;
        private TextBox txtMail;
        private ComboBox cmbRol;
        private Button btnGenPki;
        private PictureBox _icoCert;
        private PictureBox _icoPriv;
        private PictureBox _icoCfg;
        private Button btnCreate;

        private ListBox lstLog;
    }
}

/*

namespace RUSBP_Admin.Forms.Vistas
{
    partial class UsbAssignmentView
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // UsbAssignmentView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UsbAssignmentView";
            Size = new Size(460, 429);
            ResumeLayout(false);
        }

        #endregion
    }
}


*/