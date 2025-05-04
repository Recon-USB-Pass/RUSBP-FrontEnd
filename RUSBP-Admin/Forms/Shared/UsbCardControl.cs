using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using RUSBP_Admin.Core.Models;

namespace RUSBP_Admin.Forms.Shared
{
    public partial class UsbCardControl : UserControl
    {
        public event EventHandler? CardClicked;

        public UsbCardControl()
        {
            InitializeComponent();
            this.Click += RaiseClick;
            foreach (Control c in Controls)
                c.Click += RaiseClick;
        }

        private void RaiseClick(object? sender, EventArgs e) => CardClicked?.Invoke(this, EventArgs.Empty);

        public Employee? Employee { get; private set; }

        public void LoadData(Employee emp, string ping)
        {
            Employee = emp;
            lblName.Text = emp.Nombre;
            lblIp.Text = $"IP: {emp.Ip}";
            lblMac.Text = $"MAC: {emp.Mac}";
            lblPing.Text = ping;
        }
    }
}
