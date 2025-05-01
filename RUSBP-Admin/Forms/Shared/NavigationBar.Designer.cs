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
            components = new System.ComponentModel.Container();
            flow = new System.Windows.Forms.FlowLayoutPanel();
            SuspendLayout();
            // 
            // flow (contendrá botones)
            // 
            flow.Dock = System.Windows.Forms.DockStyle.Fill;
            flow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flow.WrapContents = false;
            flow.AutoScroll = true;
            flow.BackColor = System.Drawing.Color.FromArgb(20, 30, 50);
            flow.Name = "flow";
            flow.Size = new System.Drawing.Size(60, 700);
            // 
            // NavigationBar
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(flow);
            Name = "NavigationBar";
            Width = 60;
            ResumeLayout(false);
        }

        #endregion
    }
}
