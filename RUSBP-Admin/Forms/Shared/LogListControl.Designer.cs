namespace RUSBP_Admin.Forms.Shared
{
    partial class LogListControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.RichTextBox txtLogs;

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
            txtLogs = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // txtLogs
            // 
            txtLogs.BackColor = System.Drawing.Color.Black;
            txtLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            txtLogs.Font = new System.Drawing.Font("Consolas", 9F);
            txtLogs.ForeColor = System.Drawing.Color.Lime;
            txtLogs.ReadOnly = true;
            txtLogs.Name = "txtLogs";
            txtLogs.Size = new System.Drawing.Size(400, 300);
            txtLogs.Text = "";
            // 
            // LogListControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(txtLogs);
            Name = "LogListControl";
            Size = new System.Drawing.Size(400, 300);
            ResumeLayout(false);
        }

        #endregion
    }
}
