namespace RUSBP_Admin.Forms.Vistas
{
    partial class MonitoringView
    {
        private System.ComponentModel.IContainer components = null;
        private FlowLayoutPanel flpCards;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            flpCards = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // flpCards
            // 
            flpCards.AutoScroll = true;
            flpCards.Dock = DockStyle.Fill;
            flpCards.Padding = new Padding(20);
            flpCards.WrapContents = true;
            flpCards.Name = "flpCards";
            flpCards.TabIndex = 0;
            // 
            // MonitoringView
            // 
            Controls.Add(flpCards);
            Name = "MonitoringView";
            BackColor = Color.FromArgb(16, 24, 32);
            Size = new Size(800, 600);
            Load += MonitoringView_Load;
            ResumeLayout(false);
        }
    }
}






/*


namespace RUSBP_Admin.Forms.Vistas
{
    partial class MonitoringView
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
            SuspendLayout();
            // 
            // MonitoringView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "MonitoringView";
            Size = new Size(740, 335);
            ResumeLayout(false);
        }

        #endregion
    }
}


*/