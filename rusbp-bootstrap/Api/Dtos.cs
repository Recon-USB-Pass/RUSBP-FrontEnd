namespace rusbp_bootstrap.Api
{
    public enum UsbRole : byte { Root = 0, Admin = 1, Employee = 2 }

    public class UsbRegisterDto
    {
        public string Serial { get; set; } = "";
        public string Cipher { get; set; } = "";
        public string Tag { get; set; } = "";
        public UsbRole Rol { get; set; } = UsbRole.Employee;
    }

    public class UsuarioDto
    {
        public string Rut { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Depto { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "Admin";
        public string Pin { get; set; } = "";
    }

    public class UsuarioCreated
    {
        public int Id { get; set; }
        public string? Msg { get; set; }
    }
}
