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
            components = new System.ComponentModel.Container();
            picIcon = new System.Windows.Forms.PictureBox();
            lblTitle = new System.Windows.Forms.Label();
            lblValue = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            SuspendLayout();
            // 
            // picIcon
            // 
            picIcon.Location = new System.Drawing.Point(10, 10);
            picIcon.Name = "picIcon";
            picIcon.Size = new System.Drawing.Size(24, 24);
            picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picIcon.Image = Properties.Resources.logo_small;
            picIcon.TabIndex = 2;
            picIcon.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.White;
            lblTitle.Location = new System.Drawing.Point(44, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(65, 15);
            lblTitle.Text = "lblTitle";
            // 
            // lblValue
            // 
            lblValue.AutoSize = true;
            lblValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblValue.ForeColor = System.Drawing.Color.White;
            lblValue.Location = new System.Drawing.Point(44, 25);
            lblValue.Name = "lblValue";
            lblValue.Size = new System.Drawing.Size(60, 15);
            lblValue.Text = "lblValue";
            // 
            // EmployeeCardControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(46, 56, 74);
            BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            Controls.Add(picIcon);
            Controls.Add(lblTitle);
            Controls.Add(lblValue);
            Name = "EmployeeCardControl";
            Size = new System.Drawing.Size(300, 48);
            ((System.ComponentModel.ISupportInitialize)picIcon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
