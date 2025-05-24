using System;

namespace rusbp_bootstrap.Models
{
    public class ConfigJson
    {
        public string Nombre { get; set; } = "";
        public string Rut { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "Admin";
        public string Serial { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
