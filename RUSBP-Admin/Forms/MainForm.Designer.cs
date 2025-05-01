namespace RUSBP_Admin.Forms
{
    partial class MainForm
    {
        /// <summary>Variable del diseñador.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Limpieza de recursos usados.</summary>
        /// <param name="disposing">true si se deben liberar recursos administrados.</param>
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
            SuspendLayout();
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(22, 34, 48); // mismo azul oscuro
            ClientSize = new System.Drawing.Size(1280, 720);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Administrador USB";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ResumeLayout(false);
        }

        #endregion
    }
}







/*
namespace RUSBP_Admin.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "MainForm";
        }

        #endregion
    }
}
*/