
namespace RUSBP_Admin.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Shared.NavigationBar _navBar;
        private System.Windows.Forms.Panel _panelContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows-Form Designer code
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            /* ---------- NavigationBar (dock left) ---------- */
            _navBar = new Shared.NavigationBar
            {
                Name = "navigationBar",
                Dock = System.Windows.Forms.DockStyle.Left,
                Width = 90         // ancho fijo
            };

            /* ---------- Panel contenedor de vistas ---------- */
            _panelContent = new System.Windows.Forms.Panel
            {
                Name = "panelContent",
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(16, 24, 32),
                Padding = new System.Windows.Forms.Padding(10)
            };

            /* ---------- MainForm ---------- */
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(16, 24, 32);
            ClientSize = new System.Drawing.Size(1280, 768);
            MinimumSize = new System.Drawing.Size(1024, 600);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Bloqueo USB – Administrador";

            Controls.Add(_panelContent);
            Controls.Add(_navBar);
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