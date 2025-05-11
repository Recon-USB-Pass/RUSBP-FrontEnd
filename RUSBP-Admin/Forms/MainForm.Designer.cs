
using System.Drawing;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms
{
    partial class MainForm
    {
        /// <summary>Designer container</summary>
        private System.ComponentModel.IContainer components = null;

        private Shared.NavigationBar _navBar = null!;
        private Panel _panelContent = null!;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components?.Dispose();
            base.Dispose(disposing);
        }

        #region  Windows-Form Designer generated code
        private void InitializeComponent()
        {
            _navBar = new RUSBP_Admin.Forms.Shared.NavigationBar();
            _panelContent = new Panel();
            SuspendLayout();
            // 
            // _navBar
            // 
            _navBar.Dock = DockStyle.Left;
            _navBar.Location = new Point(0, 0);
            _navBar.Name = "_navBar";
            _navBar.Size = new Size(74, 768);
            _navBar.TabIndex = 1;
            // 
            // _panelContent
            // 
            _panelContent.BackColor = Color.FromArgb(16, 24, 32);
            _panelContent.Dock = DockStyle.Fill;
            _panelContent.Location = new Point(74, 0);
            _panelContent.Name = "_panelContent";
            _panelContent.Size = new Size(1206, 768);
            _panelContent.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(16, 24, 32);
            ClientSize = new Size(1280, 768);
            Controls.Add(_panelContent);
            Controls.Add(_navBar);
            MinimumSize = new Size(1024, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bloqueo USB – Administrador";
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
            navigationBar1 = new RUSBP_Admin.Forms.Shared.NavigationBar();
            usbAssignmentView1 = new RUSBP_Admin.Forms.Vistas.UsbAssignmentView();
            SuspendLayout();
            // 
            // navigationBar1
            // 
            navigationBar1.Location = new Point(-1, 0);
            navigationBar1.Name = "navigationBar1";
            navigationBar1.Size = new Size(84, 394);
            navigationBar1.TabIndex = 0;
            // 
            // usbAssignmentView1
            // 
            usbAssignmentView1.BackColor = Color.Navy;
            usbAssignmentView1.Location = new Point(224, 38);
            usbAssignmentView1.Name = "usbAssignmentView1";
            usbAssignmentView1.Size = new Size(175, 185);
            usbAssignmentView1.TabIndex = 1;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(22, 34, 48);
            ClientSize = new Size(879, 394);
            Controls.Add(usbAssignmentView1);
            Controls.Add(navigationBar1);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Administrador USB";
            WindowState = FormWindowState.Maximized;
            ResumeLayout(false);
        }

        #endregion

        private Shared.NavigationBar navigationBar1;
        private Vistas.UsbAssignmentView usbAssignmentView1;
    }
}
*/






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