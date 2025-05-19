using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUSBP_Admin.Forms.Shared
{

        /// <summary>Valores configurables en tiempo de ejecución (en memoria).</summary>
    public static class AppSettings
        {
            /// <summary>Intervalo de ping a backend en milisegundos.</summary>
            public static int PingIntervalMs { get; set; } = 10_000;   // 10 s por defecto
        }

}
