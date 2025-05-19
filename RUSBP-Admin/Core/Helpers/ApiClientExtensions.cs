// Helpers/ApiClientExtensions.cs
namespace RUSBP_Admin.Core.Services
{
    public static class ApiClientExtensions
    {
        public static Task<bool> IsUsbOnlineAsync(this ApiClient _, int __)
            => Task.FromResult(false);   // TODO: reemplazar por llamada real
    }
}
