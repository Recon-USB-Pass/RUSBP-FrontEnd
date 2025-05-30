using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Models
{
    public class LogEvent
    {
        public string EventId { get; set; } = "";
        public string UserRut { get; set; } = "";
        public string UsbSerial { get; set; } = "";
        public string EventType { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Mac { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

}
