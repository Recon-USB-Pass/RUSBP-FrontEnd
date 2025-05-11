
namespace RUSBP_Admin.Forms.Vistas
{
    partial class EmployeeDetailView
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblLogs;
        private RUSBP_Admin.Forms.Shared.LogListControl logList;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        private void InitializeComponent()
        {
            lblHeader = new Label();
            lblLogs = new Label();
            pictureBox1 = new PictureBox();
            pictureBox3 = new PictureBox();
            pictureBox4 = new PictureBox();
            pictureBox5 = new PictureBox();
            pictureBox6 = new PictureBox();
            pictureBox7 = new PictureBox();
            pictureBox8 = new PictureBox();
            pictureBox9 = new PictureBox();
            lblNombre = new Label();
            lblRut = new Label();
            lblIp = new Label();
            pictureBox2 = new PictureBox();
            logList = new RUSBP_Admin.Forms.Shared.LogListControl();
            panelLeft = new Panel();
            flpCards = new Panel();
            panel9 = new Panel();
            lblVariableEstadoPKI = new Label();
            lblEstadoPKI = new Label();
            panel8 = new Panel();
            lblVariableAlmacenamiento = new Label();
            lblAlmacenamiento = new Label();
            panel7 = new Panel();
            lblVariableCargo = new Label();
            lblCargo = new Label();
            panel6 = new Panel();
            lblVariableAreaTrabajo = new Label();
            lblAreaTrabajo = new Label();
            panel5 = new Panel();
            lblNumeroSerie = new Label();
            lblVariableNumeroSerie = new Label();
            panel4 = new Panel();
            lblVariableMac = new Label();
            lblMac = new Label();
            panel3 = new Panel();
            lblVariableIp = new Label();
            panel2 = new Panel();
            lblVariabelRut = new Label();
            panel1 = new Panel();
            lblVariableNombre = new Label();
            panelRight = new Panel();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox8).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panelLeft.SuspendLayout();
            flpCards.SuspendLayout();
            panel9.SuspendLayout();
            panel8.SuspendLayout();
            panel7.SuspendLayout();
            panel6.SuspendLayout();
            panel5.SuspendLayout();
            panel4.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            panelRight.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.AutoSize = true;
            lblHeader.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblHeader.ForeColor = Color.White;
            lblHeader.Location = new Point(73, 12);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(339, 30);
            lblHeader.TabIndex = 1;
            lblHeader.Text = "INFORMACIÓN DEL EMPLEADO";
            // 
            // lblLogs
            // 
            lblLogs.AutoSize = true;
            lblLogs.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblLogs.ForeColor = Color.White;
            lblLogs.Location = new Point(16, 12);
            lblLogs.Name = "lblLogs";
            lblLogs.Size = new Size(226, 30);
            lblLogs.TabIndex = 1;
            lblLogs.Text = "LOGS DE ACTIVIDAD";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.icon_user;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(63, 61);
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = Properties.Resources.icon_ip;
            pictureBox3.Location = new Point(0, 0);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(63, 61);
            pictureBox3.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox3.TabIndex = 2;
            pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            pictureBox4.Image = Properties.Resources.icon_mac;
            pictureBox4.Location = new Point(0, 0);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(63, 61);
            pictureBox4.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox4.TabIndex = 3;
            pictureBox4.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.Image = Properties.Resources.icon_serial;
            pictureBox5.Location = new Point(0, 0);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(63, 61);
            pictureBox5.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox5.TabIndex = 4;
            pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            pictureBox6.Image = Properties.Resources.icon_dept;
            pictureBox6.Location = new Point(0, 0);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(63, 61);
            pictureBox6.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox6.TabIndex = 5;
            pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            pictureBox7.Image = Properties.Resources.icon_role;
            pictureBox7.Location = new Point(0, 0);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(63, 61);
            pictureBox7.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox7.TabIndex = 6;
            pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            pictureBox8.Image = Properties.Resources.icon_storage;
            pictureBox8.Location = new Point(0, 0);
            pictureBox8.Name = "pictureBox8";
            pictureBox8.Size = new Size(63, 61);
            pictureBox8.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox8.TabIndex = 7;
            pictureBox8.TabStop = false;
            // 
            // pictureBox9
            // 
            pictureBox9.Image = Properties.Resources.icon_key;
            pictureBox9.Location = new Point(0, 0);
            pictureBox9.Name = "pictureBox9";
            pictureBox9.Size = new Size(63, 61);
            pictureBox9.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox9.TabIndex = 8;
            pictureBox9.TabStop = false;
            // 
            // lblNombre
            // 
            lblNombre.AutoSize = true;
            lblNombre.ForeColor = SystemColors.ControlLight;
            lblNombre.Location = new Point(69, 23);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(107, 15);
            lblNombre.TabIndex = 9;
            lblNombre.Text = "Nombre empleado";
            // 
            // lblRut
            // 
            lblRut.AutoSize = true;
            lblRut.ForeColor = SystemColors.ControlLight;
            lblRut.Location = new Point(69, 23);
            lblRut.Name = "lblRut";
            lblRut.Size = new Size(100, 15);
            lblRut.TabIndex = 10;
            lblRut.Text = "Rut del empleado";
            // 
            // lblIp
            // 
            lblIp.AutoSize = true;
            lblIp.ForeColor = SystemColors.ControlLight;
            lblIp.Location = new Point(69, 22);
            lblIp.Name = "lblIp";
            lblIp.Size = new Size(54, 15);
            lblIp.TabIndex = 11;
            lblIp.Text = "IP Actual";
            // 
            // pictureBox2
            // 
            pictureBox2.Image = Properties.Resources.icon_id;
            pictureBox2.Location = new Point(0, 0);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(63, 61);
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // logList
            // 
            logList.Dock = DockStyle.Fill;
            logList.ForeColor = SystemColors.ControlLight;
            logList.Location = new Point(0, 0);
            logList.Name = "logList";
            logList.Size = new Size(620, 700);
            logList.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.BackColor = Color.FromArgb(22, 34, 48);
            panelLeft.Controls.Add(lblHeader);
            panelLeft.Controls.Add(flpCards);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 0);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(480, 700);
            panelLeft.TabIndex = 1;
            // 
            // flpCards
            // 
            flpCards.BackColor = Color.FromArgb(22, 37, 48);
            flpCards.Controls.Add(panel9);
            flpCards.Controls.Add(panel8);
            flpCards.Controls.Add(panel7);
            flpCards.Controls.Add(panel6);
            flpCards.Controls.Add(panel5);
            flpCards.Controls.Add(panel4);
            flpCards.Controls.Add(panel3);
            flpCards.Controls.Add(panel2);
            flpCards.Controls.Add(panel1);
            flpCards.Location = new Point(16, 55);
            flpCards.Name = "flpCards";
            flpCards.Size = new Size(450, 630);
            flpCards.TabIndex = 14;
            // 
            // panel9
            // 
            panel9.BackColor = Color.FromArgb(22, 34, 70);
            panel9.Controls.Add(lblVariableEstadoPKI);
            panel9.Controls.Add(lblEstadoPKI);
            panel9.Controls.Add(pictureBox9);
            panel9.ForeColor = SystemColors.ControlLight;
            panel9.Location = new Point(22, 549);
            panel9.Name = "panel9";
            panel9.Size = new Size(405, 61);
            panel9.TabIndex = 17;
            // 
            // lblVariableEstadoPKI
            // 
            lblVariableEstadoPKI.AutoSize = true;
            lblVariableEstadoPKI.Location = new Point(329, 26);
            lblVariableEstadoPKI.Name = "lblVariableEstadoPKI";
            lblVariableEstadoPKI.Size = new Size(59, 15);
            lblVariableEstadoPKI.TabIndex = 21;
            lblVariableEstadoPKI.Text = "EstadoPKI";
            // 
            // lblEstadoPKI
            // 
            lblEstadoPKI.AutoSize = true;
            lblEstadoPKI.Location = new Point(69, 26);
            lblEstadoPKI.Name = "lblEstadoPKI";
            lblEstadoPKI.Size = new Size(95, 15);
            lblEstadoPKI.TabIndex = 21;
            lblEstadoPKI.Text = "Estado de Claves";
            // 
            // panel8
            // 
            panel8.BackColor = Color.FromArgb(22, 34, 70);
            panel8.Controls.Add(lblVariableAlmacenamiento);
            panel8.Controls.Add(lblAlmacenamiento);
            panel8.Controls.Add(pictureBox8);
            panel8.Location = new Point(22, 482);
            panel8.Name = "panel8";
            panel8.Size = new Size(405, 61);
            panel8.TabIndex = 17;
            // 
            // lblVariableAlmacenamiento
            // 
            lblVariableAlmacenamiento.AutoSize = true;
            lblVariableAlmacenamiento.ForeColor = SystemColors.ControlLight;
            lblVariableAlmacenamiento.Location = new Point(283, 21);
            lblVariableAlmacenamiento.Name = "lblVariableAlmacenamiento";
            lblVariableAlmacenamiento.Size = new Size(96, 15);
            lblVariableAlmacenamiento.TabIndex = 20;
            lblVariableAlmacenamiento.Text = "almacenamiento";
            // 
            // lblAlmacenamiento
            // 
            lblAlmacenamiento.AutoSize = true;
            lblAlmacenamiento.ForeColor = SystemColors.ControlLight;
            lblAlmacenamiento.Location = new Point(69, 21);
            lblAlmacenamiento.Name = "lblAlmacenamiento";
            lblAlmacenamiento.Size = new Size(122, 15);
            lblAlmacenamiento.TabIndex = 20;
            lblAlmacenamiento.Text = "Almacenamiento USB";
            // 
            // panel7
            // 
            panel7.BackColor = Color.FromArgb(22, 34, 70);
            panel7.Controls.Add(lblVariableCargo);
            panel7.Controls.Add(lblCargo);
            panel7.Controls.Add(pictureBox7);
            panel7.Location = new Point(22, 415);
            panel7.Name = "panel7";
            panel7.Size = new Size(405, 61);
            panel7.TabIndex = 16;
            // 
            // lblVariableCargo
            // 
            lblVariableCargo.AutoSize = true;
            lblVariableCargo.ForeColor = SystemColors.ControlLight;
            lblVariableCargo.Location = new Point(330, 21);
            lblVariableCargo.Name = "lblVariableCargo";
            lblVariableCargo.Size = new Size(37, 15);
            lblVariableCargo.TabIndex = 16;
            lblVariableCargo.Text = "cargo";
            // 
            // lblCargo
            // 
            lblCargo.AutoSize = true;
            lblCargo.ForeColor = SystemColors.ControlLight;
            lblCargo.Location = new Point(69, 21);
            lblCargo.Name = "lblCargo";
            lblCargo.Size = new Size(39, 15);
            lblCargo.TabIndex = 19;
            lblCargo.Text = "Cargo";
            // 
            // panel6
            // 
            panel6.BackColor = Color.FromArgb(22, 34, 70);
            panel6.Controls.Add(lblVariableAreaTrabajo);
            panel6.Controls.Add(lblAreaTrabajo);
            panel6.Controls.Add(pictureBox6);
            panel6.Location = new Point(22, 348);
            panel6.Name = "panel6";
            panel6.Size = new Size(405, 61);
            panel6.TabIndex = 16;
            // 
            // lblVariableAreaTrabajo
            // 
            lblVariableAreaTrabajo.AutoSize = true;
            lblVariableAreaTrabajo.ForeColor = SystemColors.ControlLight;
            lblVariableAreaTrabajo.Location = new Point(330, 20);
            lblVariableAreaTrabajo.Name = "lblVariableAreaTrabajo";
            lblVariableAreaTrabajo.Size = new Size(29, 15);
            lblVariableAreaTrabajo.TabIndex = 15;
            lblVariableAreaTrabajo.Text = "area";
            // 
            // lblAreaTrabajo
            // 
            lblAreaTrabajo.AutoSize = true;
            lblAreaTrabajo.ForeColor = SystemColors.ControlLight;
            lblAreaTrabajo.Location = new Point(69, 20);
            lblAreaTrabajo.Name = "lblAreaTrabajo";
            lblAreaTrabajo.Size = new Size(89, 15);
            lblAreaTrabajo.TabIndex = 14;
            lblAreaTrabajo.Text = "Area de Trabajo";
            // 
            // panel5
            // 
            panel5.BackColor = Color.FromArgb(22, 34, 70);
            panel5.Controls.Add(lblNumeroSerie);
            panel5.Controls.Add(lblVariableNumeroSerie);
            panel5.Controls.Add(pictureBox5);
            panel5.ForeColor = SystemColors.ControlLight;
            panel5.Location = new Point(22, 281);
            panel5.Name = "panel5";
            panel5.Size = new Size(405, 61);
            panel5.TabIndex = 18;
            // 
            // lblNumeroSerie
            // 
            lblNumeroSerie.AutoSize = true;
            lblNumeroSerie.ForeColor = SystemColors.ControlLight;
            lblNumeroSerie.Location = new Point(69, 24);
            lblNumeroSerie.Name = "lblNumeroSerie";
            lblNumeroSerie.Size = new Size(118, 15);
            lblNumeroSerie.TabIndex = 13;
            lblNumeroSerie.Text = "Numero de serie USB";
            // 
            // lblVariableNumeroSerie
            // 
            lblVariableNumeroSerie.AutoSize = true;
            lblVariableNumeroSerie.Location = new Point(330, 24);
            lblVariableNumeroSerie.Name = "lblVariableNumeroSerie";
            lblVariableNumeroSerie.Size = new Size(34, 15);
            lblVariableNumeroSerie.TabIndex = 14;
            lblVariableNumeroSerie.Text = "serial";
            // 
            // panel4
            // 
            panel4.BackColor = Color.FromArgb(22, 34, 70);
            panel4.Controls.Add(lblVariableMac);
            panel4.Controls.Add(lblMac);
            panel4.Controls.Add(pictureBox4);
            panel4.Location = new Point(22, 214);
            panel4.Name = "panel4";
            panel4.Size = new Size(405, 61);
            panel4.TabIndex = 17;
            // 
            // lblVariableMac
            // 
            lblVariableMac.AutoSize = true;
            lblVariableMac.ForeColor = SystemColors.ControlLight;
            lblVariableMac.Location = new Point(330, 23);
            lblVariableMac.Name = "lblVariableMac";
            lblVariableMac.Size = new Size(30, 15);
            lblVariableMac.TabIndex = 13;
            lblVariableMac.Text = "mac";
            // 
            // lblMac
            // 
            lblMac.AutoSize = true;
            lblMac.ForeColor = SystemColors.ControlLight;
            lblMac.Location = new Point(69, 23);
            lblMac.Name = "lblMac";
            lblMac.Size = new Size(107, 15);
            lblMac.TabIndex = 12;
            lblMac.Text = "Dirección Mac USB";
            // 
            // panel3
            // 
            panel3.BackColor = Color.FromArgb(22, 34, 70);
            panel3.Controls.Add(lblVariableIp);
            panel3.Controls.Add(pictureBox3);
            panel3.Controls.Add(lblIp);
            panel3.Location = new Point(22, 147);
            panel3.Name = "panel3";
            panel3.Size = new Size(405, 61);
            panel3.TabIndex = 16;
            // 
            // lblVariableIp
            // 
            lblVariableIp.AutoSize = true;
            lblVariableIp.ForeColor = SystemColors.ControlLight;
            lblVariableIp.Location = new Point(330, 22);
            lblVariableIp.Name = "lblVariableIp";
            lblVariableIp.Size = new Size(17, 15);
            lblVariableIp.TabIndex = 12;
            lblVariableIp.Text = "ip";
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(22, 34, 70);
            panel2.Controls.Add(lblVariabelRut);
            panel2.Controls.Add(pictureBox2);
            panel2.Controls.Add(lblRut);
            panel2.Location = new Point(22, 80);
            panel2.Name = "panel2";
            panel2.Size = new Size(405, 61);
            panel2.TabIndex = 15;
            // 
            // lblVariabelRut
            // 
            lblVariabelRut.AutoSize = true;
            lblVariabelRut.ForeColor = SystemColors.ControlLight;
            lblVariabelRut.Location = new Point(330, 23);
            lblVariabelRut.Name = "lblVariabelRut";
            lblVariabelRut.Size = new Size(22, 15);
            lblVariabelRut.TabIndex = 11;
            lblVariabelRut.Text = "rut";
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(22, 34, 70);
            panel1.Controls.Add(lblVariableNombre);
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(lblNombre);
            panel1.Location = new Point(22, 13);
            panel1.Name = "panel1";
            panel1.Size = new Size(405, 61);
            panel1.TabIndex = 14;
            // 
            // lblVariableNombre
            // 
            lblVariableNombre.AutoSize = true;
            lblVariableNombre.ForeColor = SystemColors.ControlLight;
            lblVariableNombre.Location = new Point(330, 23);
            lblVariableNombre.Name = "lblVariableNombre";
            lblVariableNombre.Size = new Size(49, 15);
            lblVariableNombre.TabIndex = 10;
            lblVariableNombre.Text = "nombre";
            // 
            // panelRight
            // 
            panelRight.BackColor = Color.FromArgb(22, 34, 48);
            panelRight.Controls.Add(logList);
            panelRight.Controls.Add(lblLogs);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(480, 0);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(620, 700);
            panelRight.TabIndex = 0;
            // 
            // EmployeeDetailView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(22, 34, 48);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            Name = "EmployeeDetailView";
            Size = new Size(1100, 700);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox8).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            flpCards.ResumeLayout(false);
            panel9.ResumeLayout(false);
            panel9.PerformLayout();
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;
        private PictureBox pictureBox7;
        private PictureBox pictureBox8;
        private PictureBox pictureBox9;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Label lblNombre;
        private Label lblRut;
        private Label lblIp;
        private Panel flpCards;
        private Panel panel2;
        private Panel panel1;
        private Panel panel7;
        private Panel panel6;
        private Panel panel5;
        private Panel panel4;
        private Panel panel3;
        private Label lblMac;
        private Panel panel9;
        private Panel panel8;
        private Label lblEstadoPKI;
        private Label lblAlmacenamiento;
        private Label lblCargo;
        private Label lblAreaTrabajo;
        private Label lblVariableNombre;
        private Label lblVariableEstadoPKI;
        private Label lblVariableAlmacenamiento;
        private Label lblVariableCargo;
        private Label lblVariableAreaTrabajo;
        private Label lblVariableNumeroSerie;
        private Label lblVariableMac;
        private Label lblVariableIp;
        private Label lblVariabelRut;
        private Label lblNumeroSerie;
    }
}



/*

namespace RUSBP_Admin.Forms.Vistas
{
    partial class EmployeeDetailView
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblLogs;
        private System.Windows.Forms.FlowLayoutPanel flpCards;
        private RUSBP_Admin.Forms.Shared.LogListControl logList;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblHeader = new System.Windows.Forms.Label();
            lblLogs = new System.Windows.Forms.Label();
            flpCards = new System.Windows.Forms.FlowLayoutPanel();
            logList = new RUSBP_Admin.Forms.Shared.LogListControl();
            panelLeft = new System.Windows.Forms.Panel();
            panelRight = new System.Windows.Forms.Panel();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.AutoSize = true;
            lblHeader.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblHeader.ForeColor = System.Drawing.Color.White;
            lblHeader.Location = new System.Drawing.Point(16, 12);
            lblHeader.Text = "INFORMACIÓN DEL EMPLEADO";
            // 
            // lblLogs
            // 
            lblLogs.AutoSize = true;
            lblLogs.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            lblLogs.ForeColor = System.Drawing.Color.White;
            lblLogs.Location = new System.Drawing.Point(16, 12);
            lblLogs.Text = "LOGS DE ACTIVIDAD";
            // 
            // flpCards
            // 
            flpCards.Anchor = System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                            | System.Windows.Forms.AnchorStyles.Right;
            flpCards.AutoScroll = true;
            flpCards.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flpCards.WrapContents = false;
            flpCards.Location = new System.Drawing.Point(16, 56);
            flpCards.Size = new System.Drawing.Size(460, 600);
            // 
            // logList
            // 
            logList.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // panelLeft
            // 
            panelLeft.BackColor = System.Drawing.Color.FromArgb(22, 34, 48);
            panelLeft.Controls.Add(flpCards);
            panelLeft.Controls.Add(lblHeader);
            panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            panelLeft.Width = 500;
            // 
            // panelRight
            // 
            panelRight.BackColor = System.Drawing.Color.FromArgb(22, 34, 48);
            panelRight.Controls.Add(logList);
            panelRight.Controls.Add(lblLogs);
            panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // EmployeeDetailView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(22, 34, 48);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            Name = "EmployeeDetailView";
            Size = new System.Drawing.Size(1100, 700);
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}



/*
namespace RUSBP_Admin.Forms.Vistas
{
    partial class EmployeeDetailView
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
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion
    }
}

*/