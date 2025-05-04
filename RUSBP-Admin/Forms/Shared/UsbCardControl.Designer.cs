namespace RUSBP_Admin.Forms.Shared
{
    partial class UsbCardControl
    {
        private System.ComponentModel.IContainer components = null;
        private PictureBox picUsb;
        private Label lblName;
        private Label lblIp;
        private Label lblMac;
        private Label lblPing;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            picUsb = new PictureBox();
            lblName = new Label();
            lblIp = new Label();
            lblMac = new Label();
            lblPing = new Label();
            ((System.ComponentModel.ISupportInitialize)picUsb).BeginInit();
            SuspendLayout();
            // 
            // picUsb
            // 
            picUsb.Image = Properties.Resources.usb_icon_off;
            picUsb.Location = new Point(20, 18);
            picUsb.Name = "picUsb";
            picUsb.Size = new Size(48, 48);
            picUsb.SizeMode = PictureBoxSizeMode.Zoom;
            picUsb.TabIndex = 0;
            picUsb.TabStop = false;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblName.ForeColor = Color.White;
            lblName.Location = new Point(80, 20);
            lblName.Name = "lblName";
            lblName.Size = new Size(56, 21);
            lblName.TabIndex = 1;
            lblName.Text = "Name";
            // 
            // lblIp
            // 
            lblIp.AutoSize = true;
            lblIp.ForeColor = Color.White;
            lblIp.Location = new Point(80, 50);
            lblIp.Name = "lblIp";
            lblIp.Size = new Size(17, 15);
            lblIp.TabIndex = 2;
            lblIp.Text = "Ip";
            // 
            // lblMac
            // 
            lblMac.AutoSize = true;
            lblMac.ForeColor = Color.White;
            lblMac.Location = new Point(80, 70);
            lblMac.Name = "lblMac";
            lblMac.Size = new Size(30, 15);
            lblMac.TabIndex = 3;
            lblMac.Text = "Mac";
            // 
            // lblPing
            // 
            lblPing.AutoSize = true;
            lblPing.ForeColor = Color.White;
            lblPing.Location = new Point(80, 90);
            lblPing.Name = "lblPing";
            lblPing.Size = new Size(31, 15);
            lblPing.TabIndex = 4;
            lblPing.Text = "Ping";
            // 
            // UsbCardControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(25, 35, 45);
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(picUsb);
            Controls.Add(lblName);
            Controls.Add(lblIp);
            Controls.Add(lblMac);
            Controls.Add(lblPing);
            Name = "UsbCardControl";
            Size = new Size(280, 120);
            ((System.ComponentModel.ISupportInitialize)picUsb).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}



/*


namespace RUSBP_Admin.Forms.Shared
{
    partial class UsbCardControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox picUsb;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblIp;
        private System.Windows.Forms.Label lblMac;
        private System.Windows.Forms.Label lblPing;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        private void InitializeComponent()
        {
            picUsb = new PictureBox();
            lblName = new Label();
            lblIp = new Label();
            lblMac = new Label();
            lblPing = new Label();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picUsb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // picUsb
            // 
            picUsb.Image = Properties.Resources.usb_off;
            picUsb.Location = new Point(12, 18);
            picUsb.Name = "picUsb";
            picUsb.Size = new Size(48, 48);
            picUsb.SizeMode = PictureBoxSizeMode.Zoom;
            picUsb.TabIndex = 0;
            picUsb.TabStop = false;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblName.ForeColor = Color.White;
            lblName.Location = new Point(70, 12);
            lblName.Name = "lblName";
            lblName.Size = new Size(76, 21);
            lblName.TabIndex = 1;
            lblName.Text = "lblName";
            // 
            // lblIp
            // 
            lblIp.AutoSize = true;
            lblIp.ForeColor = Color.White;
            lblIp.Location = new Point(70, 38);
            lblIp.Name = "lblIp";
            lblIp.Size = new Size(30, 15);
            lblIp.TabIndex = 2;
            lblIp.Text = "lblIp";
            // 
            // lblMac
            // 
            lblMac.AutoSize = true;
            lblMac.ForeColor = Color.White;
            lblMac.Location = new Point(70, 58);
            lblMac.Name = "lblMac";
            lblMac.Size = new Size(43, 15);
            lblMac.TabIndex = 3;
            lblMac.Text = "lblMac";
            // 
            // lblPing
            // 
            lblPing.AutoSize = true;
            lblPing.ForeColor = Color.White;
            lblPing.Location = new Point(70, 78);
            lblPing.Name = "lblPing";
            lblPing.Size = new Size(44, 15);
            lblPing.TabIndex = 4;
            lblPing.Text = "lblPing";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.usb_on;
            pictureBox1.Location = new Point(12, 89);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(48, 48);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // UsbCardControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(pictureBox1);
            Controls.Add(picUsb);
            Controls.Add(lblName);
            Controls.Add(lblIp);
            Controls.Add(lblMac);
            Controls.Add(lblPing);
            Name = "UsbCardControl";
            Size = new Size(447, 293);
            ((System.ComponentModel.ISupportInitialize)picUsb).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
    }
}


*/