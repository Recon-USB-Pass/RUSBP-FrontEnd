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
