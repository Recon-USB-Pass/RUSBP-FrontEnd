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
            Btn_Logout = new PictureBox();
            flow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Profile).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Log_History).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Monitoring).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Assingment_Usb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Btn_Logout).BeginInit();
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
            flow.Controls.Add(Btn_Logout);
            flow.Dock = DockStyle.Fill;
            flow.FlowDirection = FlowDirection.TopDown;
            flow.Location = new Point(0, 0);
            flow.Name = "flow";
            flow.Size = new Size(72, 719);
            flow.TabIndex = 0;
            flow.WrapContents = false;
            // 
            // Profile
            // 
            Profile.Image = Properties.Resources.icon_user;
            Profile.Location = new Point(3, 3);
            Profile.Name = "Profile";
            Profile.Size = new Size(63, 59);
            Profile.SizeMode = PictureBoxSizeMode.CenterImage;
            Profile.TabIndex = 0;
            Profile.TabStop = false;
            Profile.Tag = "Profile";
            Profile.Click += Pic_Click;
            // 
            // Log_History
            // 
            Log_History.Image = Properties.Resources.icon_Log_History;
            Log_History.Location = new Point(3, 68);
            Log_History.Name = "Log_History";
            Log_History.Size = new Size(63, 55);
            Log_History.SizeMode = PictureBoxSizeMode.CenterImage;
            Log_History.TabIndex = 1;
            Log_History.TabStop = false;
            Log_History.Tag = "Logs";
            Log_History.Click += Pic_Click;
            // 
            // Monitoring
            // 
            Monitoring.Image = Properties.Resources.icon_monitoring;
            Monitoring.Location = new Point(3, 129);
            Monitoring.Name = "Monitoring";
            Monitoring.Size = new Size(63, 61);
            Monitoring.SizeMode = PictureBoxSizeMode.CenterImage;
            Monitoring.TabIndex = 2;
            Monitoring.TabStop = false;
            Monitoring.Tag = "Monitor";
            Monitoring.Click += Pic_Click;
            // 
            // Assingment_Usb
            // 
            Assingment_Usb.Image = Properties.Resources.icon_assingment_usb;
            Assingment_Usb.Location = new Point(3, 196);
            Assingment_Usb.Name = "Assingment_Usb";
            Assingment_Usb.Size = new Size(63, 61);
            Assingment_Usb.SizeMode = PictureBoxSizeMode.CenterImage;
            Assingment_Usb.TabIndex = 3;
            Assingment_Usb.TabStop = false;
            Assingment_Usb.Tag = "Assign";
            Assingment_Usb.Click += Pic_Click;
            // 
            // Btn_Logout
            // 
            Btn_Logout.Image = Properties.Resources.icon_logout;
            Btn_Logout.Location = new Point(3, 263);
            Btn_Logout.Name = "Btn_Logout";
            Btn_Logout.Size = new Size(63, 70);
            Btn_Logout.SizeMode = PictureBoxSizeMode.CenterImage;
            Btn_Logout.TabIndex = 9;
            Btn_Logout.TabStop = false;
            Btn_Logout.Click += Pic_Click;
            // 
            // NavigationBar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flow);
            Name = "NavigationBar";
            Size = new Size(72, 719);
            flow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Profile).EndInit();
            ((System.ComponentModel.ISupportInitialize)Log_History).EndInit();
            ((System.ComponentModel.ISupportInitialize)Monitoring).EndInit();
            ((System.ComponentModel.ISupportInitialize)Assingment_Usb).EndInit();
            ((System.ComponentModel.ISupportInitialize)Btn_Logout).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private PictureBox Log_History;
        private PictureBox Monitoring;
        private PictureBox Assingment_Usb;
        private PictureBox Profile;
        private PictureBox Btn_Logout;
    }
}
