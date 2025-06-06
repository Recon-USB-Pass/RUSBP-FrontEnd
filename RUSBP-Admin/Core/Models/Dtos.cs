using System;
using System.Collections.Generic;

namespace RUSBP_Admin.Core.Models.Dtos
{
    public record AsignarDto(string Serial, string UsuarioRut);


    /// <summary>
    /// Payload de login tradicional (NO challenge), legado.
    /// </summary>
    public record LoginRequestDto(int UsuarioId, string Pin, string MacAddress);

    /// <summary>
    /// Payload para asignar un USB a un usuario (vía POST /api/usb/asignar).
    /// </summary>
    public record UsbAsignarRequestDto(string Serial, string UsuarioRut);

    /// <summary>
    /// Payload para creación/registro de USB.
    /// </summary>
    public record CrearUsbDto(string Serial, string Thumbprint);

    /// <summary>
    /// Payload para registrar la clave de recuperación cifrada (POST /api/usb/register).
    /// </summary>
    public record RegisterDto(string Serial, string Cipher, string Tag, int Rol);

    /// <summary>
    /// Payload para challenge-response (POST /api/auth/recover).
    /// </summary>
    public record RecoverDto(
    string Serial,
    string SignatureBase64,
    string Pin,
    int AgentType,
    string MacAddress
    );


    /// <summary>
    /// Respuesta de login seguro (challenge-response).
    /// </summary>
    public record RecoverResponseDto(
    string Cipher,
    string Tag,
    int Rol,
    UsuarioDto Usuario
    );


    /// <summary>
    /// Payload de creación de usuario.
    /// </summary>
    public record CrearDto(
        string Rut,
        string Nombre,
        string Depto,
        string Email,
        string Rol,
        string? PinHash,
        string Pin
    );

    /// <summary>
    /// Payload de asignación directa (POST /api/usb/{serial}/link/{rut}).
    /// </summary>
    public record LinkUsbDto(string Serial, string UsuarioRut);

    /// <summary>
    /// Usuario y su lista de dispositivos asociados.
    /// </summary>
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
        public string PinHash { get; set; } = ""; // No mostrar en UI
        public List<DispositivoUsbDto> Usbs { get; set; } = new();
        public string Serial { get; set; } = "";
        public string Area { get; set; } = "";
        public string Role { get; set; } = "";
        public int StoragePercent { get; set; }     // Ej: 0-100 para barra de almacenamiento
        public string PkiStatus { get; set; } = ""; // Ej: "Al Día", "Vencido", etc.
    }

    /// <summary>
    /// DTO para info de un USB asociado a usuario.
    /// </summary>
    public class DispositivoUsbDto
    {
        public int Id { get; set; }
        public string Serial { get; set; } = "";
        public string CertThumbprint { get; set; } = "";
        public DateTime FechaAsignacion { get; set; }
    }

    /// <summary>
    /// Payload de evento de log (envío de logs por agente).
    /// </summary>
    public record LogEventDto(
        string EventId,
        string UserRut,
        string UsbSerial,
        string EventType,
        string Ip,
        string Mac,
        DateTime Timestamp
    );

    /// <summary>
    /// Evento de log de actividad para monitoreo/consulta.
    /// </summary>
    public class LogActividadDto
    {
        public int Id { get; set; }
        public string EventId { get; set; } = "";
        public string UserRut { get; set; } = "";
        public string UsbSerial { get; set; } = "";
        public string EventType { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Mac { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string? Detalle { get; set; }

        // Alias para compatibilidad en UI
        public string Message => Detalle ?? "";
    }
}
