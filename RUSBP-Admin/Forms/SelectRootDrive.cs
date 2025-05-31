using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RUSBP_Admin.Forms
{
    /// <summary>
    /// Ventana modal para seleccionar entre varias unidades cifradas con BitLocker.
    /// </summary>
    public class SelectRootDrive : Form
    {
        public string? SelectedDriveLetter { get; private set; }

        public SelectRootDrive(List<DriveInfo> drives)
        {
            Text = "Seleccione USB Root";
            Size = new Size(500, 320);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;

            var lbl = new Label
            {
                Text = "Seleccione la unidad que corresponde al USB Root:",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 12, 0, 0)
            };

            var lst = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12),
                Height = 160
            };

            foreach (var d in drives)
            {
                var line = $"{d.VolumeLabel} ({d.Name.TrimEnd('\\')}) - {d.TotalSize / 1_073_741_824}GB";
                lst.Items.Add(line);
            }
            lst.SelectedIndex = 0;

            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 60 };
            var btnOk = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Width = 120,
                Height = 38,
                Left = 120,
                Top = 10
            };
            var btnCancel = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Width = 120,
                Height = 38,
                Left = 260,
                Top = 10
            };
            btnPanel.Controls.Add(btnOk);
            btnPanel.Controls.Add(btnCancel);

            btnOk.Click += (s, e) =>
            {
                if (lst.SelectedIndex >= 0)
                {
                    SelectedDriveLetter = drives[lst.SelectedIndex].Name.TrimEnd('\\');
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };
            btnCancel.Click += (s, e) =>
            {
                SelectedDriveLetter = null;
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(lst);
            Controls.Add(btnPanel);
            Controls.Add(lbl);
        }

        /// <summary>
        /// Llama a este método para mostrar y obtener el drive seleccionado.
        /// </summary>
        public static string? SelectDrive(List<DriveInfo> drives)
        {
            using var dlg = new SelectRootDrive(drives);
            return dlg.ShowDialog() == DialogResult.OK ? dlg.SelectedDriveLetter : null;
        }
    }
}
