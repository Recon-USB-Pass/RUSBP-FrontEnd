using System;

namespace rusbp_bootstrap.Models
{
    public class UsbInfo
    {
        public string DriveLetter { get; set; } = "";      // Ej: "F"
        public string Serial { get; set; } = "";
        public string VolumeLabel { get; set; } = "";
        public long TotalSizeBytes { get; set; }
        public bool IsReady { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        // Puedes agregar otras propiedades relevantes (tipo, modelo, etc) si luego lo necesitas
    }
}
