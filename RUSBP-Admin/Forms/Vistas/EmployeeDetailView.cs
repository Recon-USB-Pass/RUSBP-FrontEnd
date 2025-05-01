using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RUSBP_Admin.Core.Models;
using RUSBP_Admin.Forms.Shared;

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class EmployeeDetailView : UserControl
    {
        public EmployeeDetailView() => InitializeComponent();

        /* --------------------------------------------------------------
         *  API – carga de datos reales desde tu servicio/DB
         * --------------------------------------------------------------*/
        public void LoadEmployee(Employee emp, IEnumerable<UsbLogEntry> logs)
        {
            // limpia tarjetas anteriores
            flpCards.Controls.Clear();

            // crea las tarjetas con datos
            flpCards.Controls.Add(Card("Nombre", emp.FullName, Properties.Resources.icon_user));
            flpCards.Controls.Add(Card("RUT", emp.Rut, Properties.Resources.icon_id));
            flpCards.Controls.Add(Card("IP Asignada", emp.Ip, Properties.Resources.icon_ip));
            flpCards.Controls.Add(Card("Dirección MAC USB", emp.Mac, Properties.Resources.icon_mac));
            flpCards.Controls.Add(Card("Num. Serie USB", emp.Serial, Properties.Resources.icon_serial));
            flpCards.Controls.Add(Card("Área de Trabajo", emp.Area, Properties.Resources.icon_dept));
            flpCards.Controls.Add(Card("Cargo", emp.Role, Properties.Resources.icon_role));

            // almacenamiento (barra de progreso)
            var bar = new ProgressBar { Value = emp.StoragePercent, Dock = DockStyle.Fill, Height = 18 };
            var cardStorage = Card("Almacenamiento USB", "", Properties.Resources.icon_storage);
            cardStorage.Controls.Add(bar);                    // añade la barra
            flpCards.Controls.Add(cardStorage);

            // estado PKI
            var cardPki = Card("Estado de Claves PKI", emp.PkiStatus, Properties.Resources.icon_key);
            cardPki.Controls[1].ForeColor = Color.Lime;       // lblValue
            flpCards.Controls.Add(cardPki);

            // carga logs
            logList.Clear();                                  // método de extensión sencillo
            foreach (var l in logs)
                logList.Append(l.Message);
        }

        /* helper: crea EmployeeCardControl pre-configurado */
        private static EmployeeCardControl Card(string title, string value, Image? icon = null)
        {
            var c = new EmployeeCardControl { Title = title, Value = value };
            if (icon != null) c.Controls[0].BackgroundImage = icon;   // picIcon está en Controls[0]
            return c;
        }
    }
}

