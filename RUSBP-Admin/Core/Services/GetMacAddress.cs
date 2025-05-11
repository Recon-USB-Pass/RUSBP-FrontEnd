using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Services
{
    public static class GetMacAddress
    {
        public static string GetLocalMacAddress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var mac = ni.GetPhysicalAddress().ToString();
                    if (!string.IsNullOrEmpty(mac))
                        return mac;
                }
            }

            return "N/A";
        }

    }
}
