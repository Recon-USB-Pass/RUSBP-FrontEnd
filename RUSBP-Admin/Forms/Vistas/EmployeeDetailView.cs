using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Core.Models.Dtos;     // ← LogActividadDto
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms.Shared;

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class EmployeeDetailView : UserControl
    {
        public EmployeeDetailView() => InitializeComponent();



        public void LoadSingleEmployee(Employee emp)
        {
            lblNombre.Text = emp.Nombre;
            lblRut.Text = emp.Rut;
            lblIp.Text = emp.Mac;
            lblIp.Text = emp.Ip;
            lblNumeroSerie.Text = emp.Serial;
        }

        /* -----------------------------------------------------------------
         *  Carga datos del empleado + logs de actividad
         * -----------------------------------------------------------------*/
        public void LoadEmployee(Employee emp, IEnumerable<LogActividadDto> logs)
        {
            /* ---- tarjetas de información ---- */
            flpCards.Controls.Clear();

            flpCards.Controls.Add(Card("Nombre", emp.Nombre, Properties.Resources.icon_user));
            flpCards.Controls.Add(Card("RUT", emp.Rut, Properties.Resources.icon_id));
            flpCards.Controls.Add(Card("IP Asignada", emp.Ip, Properties.Resources.icon_ip));
            flpCards.Controls.Add(Card("Dirección MAC USB", emp.Mac, Properties.Resources.icon_mac));
            flpCards.Controls.Add(Card("Num. Serie USB", emp.Serial, Properties.Resources.icon_serial));
            flpCards.Controls.Add(Card("Área de Trabajo", emp.Area, Properties.Resources.icon_dept));
            flpCards.Controls.Add(Card("Cargo", emp.Role, Properties.Resources.icon_role));

            lblNombre.Text = emp.Nombre;
            lblRut.Text = emp.Rut;
            lblIp.Text = emp.Mac;
            lblIp.Text = emp.Ip;
            lblNumeroSerie.Text = emp.Serial;

            /* ---- almacenamiento (barra de progreso) ---- */
            var bar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Height = 18,
                Value = emp.StoragePercent
            };
            var storageCard = Card("Almacenamiento USB", "", Properties.Resources.icon_storage);
            storageCard.Controls.Add(bar);
            flpCards.Controls.Add(storageCard);

            /* ---- estado PKI ---- */
            var pkiCard = Card("Estado de Claves PKI", emp.PkiStatus, Properties.Resources.icon_key);
            pkiCard.Controls[1].ForeColor = Color.Lime;   // Value label está en Controls[1]
            flpCards.Controls.Add(pkiCard);

            /* ---- logs ---- */
            logList.Clear();
            foreach (var l in logs)
                logList.Append(l.Message);
        }

        /* helper: crea una tarjeta lista para agregar al FlowLayout */
        private static EmployeeCardControl Card(string title, string value, Image? icon = null)
        {
            var c = new EmployeeCardControl { Title = title, Value = value };
            if (icon is not null)
                c.Controls[0].BackgroundImage = icon;     // picIcon está en Controls[0]
            return c;
        }

        private void logList_Load(object sender, EventArgs e)
        {

        }
    }
}
