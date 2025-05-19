using System;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms.Shared
{
    public partial class NavigationBar : UserControl
    {
        /// <summary>
        /// Se dispara cuando el usuario hace clic en un icono.
        /// Los tags devueltos son: "Monitor", "Assign", "Logs", "Profile", "Logout".
        /// </summary>
        public event Action<string>? SectionSelected;

        public NavigationBar()
        {
            InitializeComponent();
            
            // Asignar tags y wire-up en un solo sitio
            Profile.Tag = "Profile";
            Log_History.Tag = "Logs";
            Monitoring.Tag = "Monitor";
            Assingment_Usb.Tag = "Assign";
            Btn_Logout.Tag = "Logout";
            
        }

        /* ───────────────────────── Eventos ───────────────────────── */
        private void Pic_Click(object? sender, EventArgs e)
        {
            if (sender is PictureBox pic && pic.Tag is string tag)
                SectionSelected?.Invoke(tag);
        }
    }
}
