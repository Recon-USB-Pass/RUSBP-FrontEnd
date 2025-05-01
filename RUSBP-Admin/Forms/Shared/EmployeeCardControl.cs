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
    public partial class EmployeeCardControl : UserControl
    {
        public EmployeeCardControl()
        {
            InitializeComponent();
        }

        /* ---------- API pública sencilla ---------- */
        public string Title
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }

        public string Value
        {
            get => lblValue.Text;
            set => lblValue.Text = value;
        }
    }
}

