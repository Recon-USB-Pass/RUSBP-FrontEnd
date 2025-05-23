using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Models.Dtos
{
    /// <summary>
    ///  Payload que el agente envía a  POST /api/Auth/login
    /// </summary>
    public record LoginRequestDto(int UsuarioId, string Pin, string MacAddress);

    public record UsbAsignarRequestDto(string Serial, string CertThumbprint, int UsuarioId);

    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Rut { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Ip { get; set; }
        public string? Mac { get; set; }
        public string? Depto { get; set; }
        public string? Email { get; set; }
        public string? Rol { get; set; }
        public string PinHash { get; set; } = "";      // Nunca se muestra
        public List<DispositivoUsbDto> Usbs { get; set; } = [];
    }

    public class DispositivoUsbDto
    {
        public int Id { get; set; }
        public string Serial { get; set; } = "";
        public string CertThumbprint { get; set; } = "";
        public DateTime FechaAsignacion { get; set; }
    }

    public class LogActividadDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string TipoEvento { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Mac { get; set; } = "";
        public DateTime FechaHora { get; set; }
        public string? Detalle { get; set; }

        /* Alias para compatibilidad con la vista */
        public string Message => Detalle ?? "";
    }
}
