using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace RUSBP_Admin.Core.Helpers
{
    /// <summary>
    /// Utilidad para obtener la IP local IPv4 de la máquina, excluyendo loopback y direcciones no asignadas.
    /// </summary>
    public static class GetIpAddress
    {
        /// <summary>
        /// Devuelve la primera IP local IPv4 encontrada que no sea loopback.
        /// Si no encuentra ninguna, retorna string vacío.
        /// </summary>
        public static string GetLocalIpAddress()
        {
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus != OperationalStatus.Up)
                    continue;

                foreach (UnicastIPAddressInformation addr in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(addr.Address))
                    {
                        return addr.Address.ToString();
                    }
                }
            }
            return string.Empty;
        }
    }
}
