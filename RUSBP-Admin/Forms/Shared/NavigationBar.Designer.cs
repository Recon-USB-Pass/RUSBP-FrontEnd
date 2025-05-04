namespace RUSBP_Admin.Forms.Shared
{
    partial class NavigationBar
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel flow;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        private void InitializeComponent()
        {
            flow = new FlowLayoutPanel();
            Profile = new PictureBox();
            Log_History = new PictureBox();
            Monitoring = new PictureBox();
            Assingment_Usb = new PictureBox();
            Logout = new PictureBox();
            flow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Profile).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Log_History).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Monitoring).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Assingment_Usb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Logout).BeginInit();
            SuspendLayout();
            // 
            // flow
            // 
            flow.AutoScroll = true;
            flow.BackColor = Color.FromArgb(20, 30, 50);
            flow.Controls.Add(Profile);
            flow.Controls.Add(Log_History);
            flow.Controls.Add(Monitoring);
            flow.Controls.Add(Assingment_Usb);
            flow.Controls.Add(Logout);
            flow.Dock = DockStyle.Fill;
            flow.FlowDirection = FlowDirection.TopDown;
            flow.Location = new Point(0, 0);
            flow.Name = "flow";
            flow.Size = new Size(174, 911);
            flow.TabIndex = 0;
            flow.WrapContents = false;
            // 
            // Profile
            // 
            Profile.Image = Properties.Resources.icon_user;
            Profile.Location = new Point(3, 3);
            Profile.Name = "Profile";
            Profile.Size = new Size(63, 59);
            Profile.SizeMode = PictureBoxSizeMode.Zoom;
            Profile.TabIndex = 0;
            Profile.TabStop = false;
            Profile.Click += Profile_Click;
            // 
            // Log_History
            // 
            Log_History.Image = Properties.Resources.icon_Log_History;
            Log_History.Location = new Point(3, 68);
            Log_History.Name = "Log_History";
            Log_History.Size = new Size(63, 55);
            Log_History.SizeMode = PictureBoxSizeMode.Zoom;
            Log_History.TabIndex = 1;
            Log_History.TabStop = false;
            Log_History.Click += Log_History_Click;
            // 
            // Monitoring
            // 
            Monitoring.Image = Properties.Resources.icon_monitoring;
            Monitoring.Location = new Point(3, 129);
            Monitoring.Name = "Monitoring";
            Monitoring.Size = new Size(63, 61);
            Monitoring.SizeMode = PictureBoxSizeMode.Zoom;
            Monitoring.TabIndex = 2;
            Monitoring.TabStop = false;
            Monitoring.Click += Monitoring_Click;
            // 
            // Assingment_Usb
            // 
            Assingment_Usb.Image = Properties.Resources.icon_assingment_usb;
            Assingment_Usb.Location = new Point(3, 196);
            Assingment_Usb.Name = "Assingment_Usb";
            Assingment_Usb.Size = new Size(63, 61);
            Assingment_Usb.SizeMode = PictureBoxSizeMode.Zoom;
            Assingment_Usb.TabIndex = 3;
            Assingment_Usb.TabStop = false;
            Assingment_Usb.Click += Assingment_Usb_Click;
            // 
            // Logout
            // 
            Logout.Image = Properties.Resources.icon_logout;
            Logout.Location = new Point(3, 263);
            Logout.Name = "Logout";
            Logout.Size = new Size(63, 70);
            Logout.SizeMode = PictureBoxSizeMode.Zoom;
            Logout.TabIndex = 4;
            Logout.TabStop = false;
            Logout.Click += Logout_Click;
            // 
            // NavigationBar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flow);
            Name = "NavigationBar";
            Size = new Size(174, 911);
            flow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Profile).EndInit();
            ((System.ComponentModel.ISupportInitialize)Log_History).EndInit();
            ((System.ComponentModel.ISupportInitialize)Monitoring).EndInit();
            ((System.ComponentModel.ISupportInitialize)Assingment_Usb).EndInit();
            ((System.ComponentModel.ISupportInitialize)Logout).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox Profile;
        private PictureBox Log_History;
        private PictureBox Monitoring;
        private PictureBox Assingment_Usb;
        private PictureBox Logout;
    }
}
