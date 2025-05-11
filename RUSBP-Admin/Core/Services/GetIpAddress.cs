using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;

namespace RUSBP_Admin.Core.Services
{
    public static class GetIpAddress
    {
        public static string GetLocalIpAddress()
        {
            string localIP = string.Empty;

            // Obtener todas las interfaces de red disponibles en la máquina.
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Verificar si la interfaz de red está habilitada
                if (netInterface.OperationalStatus == OperationalStatus.Up)
                {
                    // Obtener las direcciones IP asociadas con la interfaz
                    foreach (UnicastIPAddressInformation addr in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        // Ignorar las direcciones IP de tipo loopback (127.0.0.1)
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(addr.Address))
                        {
                            localIP = addr.Address.ToString();
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(localIP))
                    break;
            }

            return localIP;
        }
    }
}
