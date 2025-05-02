using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RUSBP_Admin.Core.Services;

namespace RUSBP_Admin.Core.Services
{
    public class AuthService
    {
        private readonly ApiClient _api;
        public int CurrentUserId { get; private set; } = 0;

        public AuthService(ApiClient api) => _api = api;

        public async Task<bool> LoginAsync(int usuarioId, string pin, string mac)
        {
            try
            {
                await _api.LoginAsync(usuarioId, pin, mac);
                CurrentUserId = usuarioId;
                return true;
            }
            catch { return false; }
        }

    }
}

