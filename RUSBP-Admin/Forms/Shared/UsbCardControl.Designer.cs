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
            components = new System.ComponentModel.Container();
            picUsb = new System.Windows.Forms.PictureBox();
            lblName = new System.Windows.Forms.Label();
            lblIp = new System.Windows.Forms.Label();
            lblMac = new System.Windows.Forms.Label();
            lblPing = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)picUsb).BeginInit();
            SuspendLayout();
            // 
            // picUsb
            // 
            picUsb.Image = Properties.Resources.usb_icon_off;
            picUsb.Location = new System.Drawing.Point(12, 18);
            picUsb.Size = new System.Drawing.Size(48, 48);
            picUsb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblName.ForeColor = System.Drawing.Color.White;
            lblName.Location = new System.Drawing.Point(70, 12);
            lblName.Text = "lblName";
            // 
            // lblIp
            // 
            lblIp.AutoSize = true;
            lblIp.ForeColor = System.Drawing.Color.White;
            lblIp.Location = new System.Drawing.Point(70, 38);
            lblIp.Text = "lblIp";
            // 
            // lblMac
            // 
            lblMac.AutoSize = true;
            lblMac.ForeColor = System.Drawing.Color.White;
            lblMac.Location = new System.Drawing.Point(70, 58);
            lblMac.Text = "lblMac";
            // 
            // lblPing
            // 
            lblPing.AutoSize = true;
            lblPing.ForeColor = System.Drawing.Color.White;
            lblPing.Location = new System.Drawing.Point(70, 78);
            lblPing.Text = "lblPing";
            // 
            // UsbCardControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            Controls.AddRange(new System.Windows.Forms.Control[] { picUsb, lblName, lblIp, lblMac, lblPing });
            Name = "UsbCardControl";
            Size = new System.Drawing.Size(260, 110);
            ((System.ComponentModel.ISupportInitialize)picUsb).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
