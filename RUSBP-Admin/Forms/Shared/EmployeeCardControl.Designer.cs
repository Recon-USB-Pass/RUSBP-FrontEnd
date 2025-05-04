namespace RUSBP_Admin.Forms.Shared
{
    partial class EmployeeCardControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblValue;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador

        private void InitializeComponent()
        {
            picIcon = new PictureBox();
            lblTitle = new Label();
            lblValue = new Label();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            SuspendLayout();
            // 
            // picIcon
            // 
            picIcon.Image = Properties.Resources.logo_small;
            picIcon.Location = new Point(10, 10);
            picIcon.Name = "picIcon";
            picIcon.Size = new Size(24, 24);
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;
            picIcon.TabIndex = 2;
            picIcon.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(44, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(45, 15);
            lblTitle.TabIndex = 3;
            lblTitle.Text = "lblTitle";
            // 
            // lblValue
            // 
            lblValue.AutoSize = true;
            lblValue.Font = new Font("Segoe UI", 9F);
            lblValue.ForeColor = Color.White;
            lblValue.Location = new Point(44, 25);
            lblValue.Name = "lblValue";
            lblValue.Size = new Size(48, 15);
            lblValue.TabIndex = 4;
            lblValue.Text = "lblValue";
            // 
            // EmployeeCardControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(46, 56, 74);
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(picIcon);
            Controls.Add(lblTitle);
            Controls.Add(lblValue);
            Name = "EmployeeCardControl";
            Size = new Size(553, 315);
            ((System.ComponentModel.ISupportInitialize)picIcon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
