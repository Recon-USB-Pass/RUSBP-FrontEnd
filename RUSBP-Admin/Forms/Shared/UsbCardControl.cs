using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms.Shared
{
    public partial class UsbCardControl : UserControl
    {
        public UsbCardControl(string employeeName, Color back)
        {
            InitializeComponent();
            BackColor = back;
            lblName.Text = employeeName;
        }

        /* API sencilla */
        public string Ip { set => lblIp.Text = $"IP: {value}"; }
        public string Mac { set => lblMac.Text = $"MAC: {value}"; }
        public string PingTxt { set => lblPing.Text = value; }

        private void lblName_Click(object sender, EventArgs e)
        {

        }
    }
}

