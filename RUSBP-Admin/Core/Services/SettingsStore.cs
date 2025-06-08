using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RUSBP_Admin.Core.Services
{
    /// <summary>
    /// Administra el almacenamiento seguro de la clave RP_root y la IP del backend para el administrador, usando DPAPI.
    /// Si en el futuro se requiere guardar más configuraciones, se puede extender el formato de serialización.
    /// </summary>
    public static class SettingsStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RUSBP_Admin", "settings.dat");

        /// <summary>
        /// Guarda la clave RP_root y la IP del backend de forma segura (encriptada por usuario actual).
        /// </summary>
        public static void Save(string rpRoot, string backendIp)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var data = $"{rpRoot}\n{backendIp}";
            var bytes = Encoding.UTF8.GetBytes(data);
            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(FilePath, encrypted);
        }

        /// <summary>
        /// Recupera la clave RP_root y la IP del backend desde almacenamiento seguro. Devuelve null si no existe.
        /// </summary>
        public static (string rpRoot, string backendIp)? Load()
        {
            if (!File.Exists(FilePath)) return null;
            var encrypted = File.ReadAllBytes(FilePath);
            var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var parts = Encoding.UTF8.GetString(bytes).Split('\n');
            if (parts.Length >= 2)
                return (parts[0], parts[1]);
            return null;
        }

        /// <summary>
        /// Borra el archivo de configuración guardado.
        /// </summary>
        public static void Clear()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
