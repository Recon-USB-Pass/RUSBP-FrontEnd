using System.Net.NetworkInformation;

namespace RUSBP_Admin.Core.Helpers
{
    /// <summary>
    /// Utilidad para obtener la dirección MAC local (primer adaptador UP válido, no loopback).
    /// </summary>
    public static class GetMacAddress
    {
        /// <summary>
        /// Devuelve la dirección MAC del primer adaptador de red activo (no loopback) en formato XX:XX:XX:XX:XX:XX.
        /// Si no hay adaptadores, retorna "N/A".
        /// </summary>
        public static string GetLocalMacAddress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var mac = ni.GetPhysicalAddress();
                    if (mac != null && mac.GetAddressBytes().Length == 6)
                    {
                        // Formatea a "XX:XX:XX:XX:XX:XX"
                        return string.Join(":", mac.GetAddressBytes().Select(b => b.ToString("X2")));
                    }
                }
            }
            return "N/A";
        }
    }
}
