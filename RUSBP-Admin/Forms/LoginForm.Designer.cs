﻿namespace RUSBP_Admin
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
            lblStatus = new Label();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(199, 146);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(38, 15);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "label1";
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblStatus);
            Name = "LoginForm";
            Text = "LoginForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblStatus;
    }
}



/*


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
            picUsb_off = new PictureBox();
            txtPin = new TextBox();
            btnLogin = new Button();
            picUsb_on = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picUsb_off).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picUsb_on).BeginInit();
            SuspendLayout();
            // 
            // picUsb_off
            // 
            picUsb_off.BackColor = SystemColors.Window;
            picUsb_off.Image = Properties.Resources.usb_off;
            picUsb_off.Location = new Point(359, 162);
            picUsb_off.Name = "picUsb_off";
            picUsb_off.Size = new Size(200, 200);
            picUsb_off.SizeMode = PictureBoxSizeMode.Zoom;
            picUsb_off.TabIndex = 0;
            picUsb_off.TabStop = false;
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
            // picUsb_on
            // 
            picUsb_on.Image = Properties.Resources.usb_on;
            picUsb_on.Location = new Point(589, 162);
            picUsb_on.Name = "picUsb_on";
            picUsb_on.Size = new Size(200, 201);
            picUsb_on.SizeMode = PictureBoxSizeMode.Zoom;
            picUsb_on.TabIndex = 2;
            picUsb_on.TabStop = false;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Block_Screen_WALLPAPER;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1920, 918);
            Controls.Add(picUsb_on);
            Controls.Add(btnLogin);
            Controls.Add(txtPin);
            Controls.Add(picUsb_off);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)picUsb_off).EndInit();
            ((System.ComponentModel.ISupportInitialize)picUsb_on).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picUsb_off;
        private TextBox txtPin;
        private Button btnLogin;
        private PictureBox picUsb_on;
    }
}


*/