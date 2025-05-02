
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
            lblHeader = new Label();
            lblLogs = new Label();
            flpCards = new FlowLayoutPanel();
            logList = new RUSBP_Admin.Forms.Shared.LogListControl();
            panelLeft = new Panel();
            panelRight = new Panel();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox3 = new PictureBox();
            pictureBox4 = new PictureBox();
            pictureBox5 = new PictureBox();
            pictureBox6 = new PictureBox();
            pictureBox7 = new PictureBox();
            pictureBox8 = new PictureBox();
            pictureBox9 = new PictureBox();
            flpCards.SuspendLayout();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox8).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).BeginInit();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.AutoSize = true;
            lblHeader.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblHeader.ForeColor = Color.White;
            lblHeader.Location = new Point(16, 12);
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
            // flpCards
            // 
            flpCards.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flpCards.AutoScroll = true;
            flpCards.Controls.Add(pictureBox1);
            flpCards.Controls.Add(pictureBox2);
            flpCards.Controls.Add(pictureBox3);
            flpCards.Controls.Add(pictureBox4);
            flpCards.Controls.Add(pictureBox5);
            flpCards.Controls.Add(pictureBox6);
            flpCards.Controls.Add(pictureBox7);
            flpCards.Controls.Add(pictureBox8);
            flpCards.Controls.Add(pictureBox9);
            flpCards.FlowDirection = FlowDirection.TopDown;
            flpCards.Location = new Point(16, 56);
            flpCards.Name = "flpCards";
            flpCards.Size = new Size(460, 600);
            flpCards.TabIndex = 0;
            flpCards.WrapContents = false;
            // 
            // logList
            // 
            logList.Dock = DockStyle.Fill;
            logList.Location = new Point(0, 0);
            logList.Name = "logList";
            logList.Size = new Size(600, 700);
            logList.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.BackColor = Color.FromArgb(22, 34, 48);
            panelLeft.Controls.Add(flpCards);
            panelLeft.Controls.Add(lblHeader);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 0);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(500, 700);
            panelLeft.TabIndex = 1;
            // 
            // panelRight
            // 
            panelRight.BackColor = Color.FromArgb(22, 34, 48);
            panelRight.Controls.Add(logList);
            panelRight.Controls.Add(lblLogs);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(500, 0);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(600, 700);
            panelRight.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.icon_user;
            pictureBox1.Location = new Point(3, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(65, 50);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = Properties.Resources.icon_id;
            pictureBox2.Location = new Point(3, 59);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(100, 50);
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = Properties.Resources.icon_ip;
            pictureBox3.Location = new Point(3, 115);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(100, 50);
            pictureBox3.TabIndex = 2;
            pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            pictureBox4.Image = Properties.Resources.icon_mac;
            pictureBox4.Location = new Point(3, 171);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(100, 50);
            pictureBox4.TabIndex = 3;
            pictureBox4.TabStop = false;
            // 
            // pictureBox5
            // 
            pictureBox5.Image = Properties.Resources.icon_serial;
            pictureBox5.Location = new Point(3, 227);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(100, 50);
            pictureBox5.TabIndex = 4;
            pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            pictureBox6.Image = Properties.Resources.icon_dept;
            pictureBox6.Location = new Point(3, 283);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(100, 50);
            pictureBox6.TabIndex = 5;
            pictureBox6.TabStop = false;
            // 
            // pictureBox7
            // 
            pictureBox7.Image = Properties.Resources.icon_role;
            pictureBox7.Location = new Point(3, 339);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(100, 50);
            pictureBox7.TabIndex = 6;
            pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            pictureBox8.Image = Properties.Resources.icon_storage;
            pictureBox8.Location = new Point(3, 395);
            pictureBox8.Name = "pictureBox8";
            pictureBox8.Size = new Size(100, 50);
            pictureBox8.TabIndex = 7;
            pictureBox8.TabStop = false;
            // 
            // pictureBox9
            // 
            pictureBox9.Image = Properties.Resources.icon_key;
            pictureBox9.Location = new Point(3, 451);
            pictureBox9.Name = "pictureBox9";
            pictureBox9.Size = new Size(100, 50);
            pictureBox9.TabIndex = 8;
            pictureBox9.TabStop = false;
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
            flpCards.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox8).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox9).EndInit();
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