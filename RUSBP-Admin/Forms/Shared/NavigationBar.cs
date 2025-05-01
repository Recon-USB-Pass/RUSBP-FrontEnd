using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms.Shared
{
    public partial class NavigationBar : UserControl
    {
        public event Action<string>? SectionSelected;   // "Monitor" | "Assign" | "Detail" | "Logout"

        public NavigationBar() => InitializeComponent();

        private void btn_Click(object? s, EventArgs e)
        {
            if (s is Button b && b.Tag is string tag)
                SectionSelected?.Invoke(tag);
        }
    }
}

