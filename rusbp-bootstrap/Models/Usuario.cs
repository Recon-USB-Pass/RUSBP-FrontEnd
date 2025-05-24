namespace rusbp_bootstrap.Models
{
    public class Usuario
    {
        public string Rut { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Depto { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "Admin";
        public string Pin { get; set; } = "";
    }
}

