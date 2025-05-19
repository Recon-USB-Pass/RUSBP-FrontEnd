using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows.Forms;          // para MessageBox

namespace RUSBP_Admin.Core
{
    /// <summary>Carga y expone valores de appsettings.json.</summary>
    public static class AppConfig
    {
        private static readonly IConfigurationRoot _cfg;

        /* ─────────────  ctor estático  ───────────── */
        static AppConfig()
        {
            /* Ruta física donde se ejecuta el .exe */
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "appsettings.json");

            /* Diagnóstico: muestra ruta si el archivo falta */
            if (!File.Exists(path))
                MessageBox.Show($"No se encontró appsettings.json en:\n{path}",
                                "Config missing", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

            _cfg = new ConfigurationBuilder()
                      .AddJsonFile(path, optional: false, reloadOnChange: true)
                      .Build();
        }

        /* ─────────────  Propiedades públicas  ───────────── */
        public static string BackendBaseUrl =>
            _cfg["Backend:BaseUrl"]
            ?? "https://localhost:8443";    // fallback de desarrollo
    }
}
