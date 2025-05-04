using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using RUSBP_Admin.Core.Services;

namespace RUSBP_Admin.Forms.Vistas
{
    public partial class UsbAssignmentView : UserControl
    {
        private MonitoringService? _mon;
        private ApiClient? _api;

        public UsbAssignmentView() => InitializeComponent();

        /* ————— inyección de dependencias ————— */
        public void SetServices(ApiClient api) => _api = api;

        public void SetServices(MonitoringService mon, ApiClient api)
        {
            _mon = mon;
            _api = api;
        }

        /* =========================================================
         *  Ejemplo de uso de los servicios (botón “Detectar USB”)
         * =========================================================*/
    }
}
