using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RUSBP_Admin.Core.Services
{
    public static class ConfigService
    {
        private static readonly dynamic _cfg;
        static ConfigService()
        {
            _cfg = JsonSerializer.Deserialize<dynamic>(
                 File.ReadAllText("appsettings.json"));
        }
        public static string BitlockerPass => _cfg["BitLockerPassword"];
        public static string ValidPin => _cfg["Pin"];
        public static int MaxAttempts => (int)_cfg["MaxAttempts"];
    }

}
