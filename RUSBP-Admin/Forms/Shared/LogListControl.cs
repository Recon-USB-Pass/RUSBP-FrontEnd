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
    public partial class LogListControl : UserControl
    {
        public LogListControl() => InitializeComponent();

        public void Append(string line)
        {
            txtLogs.AppendText($"{line}{Environment.NewLine}");
        }
        public void Clear() => txtLogs.Clear();

    }
}

