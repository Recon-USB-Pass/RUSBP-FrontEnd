using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms.Shared
{
    public partial class NavigationBar : UserControl
    {
        public event Action<string>? SectionSelected;

        private PictureBox? _selectedPic = null;

        public NavigationBar()
        {
            InitializeComponent();

            // Asignar tags de navegación
            Profile.Tag = "Profile";
            Log_History.Tag = "Logs";
            Monitoring.Tag = "Monitor";
            Assingment_Usb.Tag = "Assign";
            Btn_Logout.Tag = "Logout";

            // Hook universal para todos los PictureBox hijos
            foreach (var pic in Controls.OfType<PictureBox>())
            {
                pic.Click += Pic_Click;
                pic.Cursor = Cursors.Hand;
                pic.BackColor = Color.Transparent; // estado inicial
            }
        }

        private void Pic_Click(object? sender, EventArgs e)
        {
            if (sender is PictureBox pic && pic.Tag is string tag)
            {
                SetActive(tag);
                SectionSelected?.Invoke(tag);
            }
        }

        // Permite marcar la sección activa desde el exterior
        public void SetActive(string tag)
        {
            foreach (var pic in Controls.OfType<PictureBox>())
            {
                bool isActive = (pic.Tag as string) == tag;
                pic.BackColor = isActive
                    ? ColorTranslator.FromHtml("#232E3A") // O tu color de resaltado
                    : Color.Transparent;
                if (isActive) _selectedPic = pic;
            }
        }
    }
}
