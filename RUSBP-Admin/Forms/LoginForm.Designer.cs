namespace RUSBP_Admin.Forms
{
    partial class LoginForm
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
            picUsb = new PictureBox();
            txtPin = new TextBox();
            btnLogin = new Button();
            ((System.ComponentModel.ISupportInitialize)picUsb).BeginInit();
            SuspendLayout();
            // 
            // picUsb
            // 
            picUsb.BackColor = SystemColors.Window;
            picUsb.Image = Properties.Resources.usb_icon_off;
            picUsb.Location = new Point(358, 188);
            picUsb.Name = "picUsb";
            picUsb.Size = new Size(200, 200);
            picUsb.SizeMode = PictureBoxSizeMode.Zoom;
            picUsb.TabIndex = 0;
            picUsb.TabStop = false;
            // 
            // txtPin
            // 
            txtPin.Location = new Point(333, 451);
            txtPin.Name = "txtPin";
            txtPin.PlaceholderText = "Contraseña";
            txtPin.Size = new Size(250, 23);
            txtPin.TabIndex = 1;
            txtPin.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.Black;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(333, 547);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(250, 40);
            btnLogin.TabIndex = 1;
            btnLogin.Text = "Entrar";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Block_Screen_WALLPAPER;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1920, 918);
            Controls.Add(btnLogin);
            Controls.Add(txtPin);
            Controls.Add(picUsb);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)picUsb).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picUsb;
        private TextBox txtPin;
        private Button btnLogin;
    }
}