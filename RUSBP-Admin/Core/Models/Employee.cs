using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Models
{
    public class Employee
    {
        // Campos que usa la UI
        public int Id { get; set; }
        public string Rut { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Area { get; set; } = "";     // no está en backend, placeholder
        public string Rol { get; set; } = "";     // idem
        public string Role { get; set; } = "";   // ← añadido
        public string Ip { get; set; } = "";     // la llena MonitoringService
        public string Mac { get; set; } = "";
        public string Serial { get; set; } = "";
        public int StoragePercent { get; set; }    // dummy, se calculará
        public string PkiStatus { get; set; } = "Al Día";
    }
}

