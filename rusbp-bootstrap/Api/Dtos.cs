using System;

namespace rusbp_bootstrap.Api
{
    // Data Transfer Objects (DTOs) para comunicación con el backend

    // DTO para USB
    public class UsbDto
    {
        public string Serial { get; set; } = "";
        public string Thumbprint { get; set; } = "";
    }

    // DTO para usuario root-admin
    public class UsuarioDto
    {
        public string Rut { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Depto { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "Admin";
        public string Pin { get; set; } = "";
    }

    // DTO para asociación USB <-> Usuario
    public class VincularUsbDto
    {
        public string Serial { get; set; } = "";
        public string UsuarioRut { get; set; } = "";
    }

    // DTO para respuesta de creación de usuario
    public class UsuarioCreated
    {
        public int Id { get; set; }
        public string? Msg { get; set; }
    }
}
