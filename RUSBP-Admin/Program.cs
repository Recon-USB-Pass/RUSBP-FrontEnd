using Microsoft.Extensions.DependencyInjection;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms;
using static RUSBP_Admin.LoginForm;
using RUSBP_Admin.Core;
using System.Diagnostics;

namespace RUSBP_Admin
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Mutex global para evitar instancias duplicadas
            using var mutex = new Mutex(true, "RUSBP_USB_LOCK_AGENT_ADMIN", out bool createdNew);
            if (!createdNew) return;          // Otra instancia ya corre

            ApplicationConfiguration.Initialize();

            /* === DI compartida === */
            var services = new ServiceCollection();

            // Base URL – mismo backend que el agente empleado
            var api = new ApiClient(AppConfig.BackendBaseUrl);

            services.AddSingleton(api);
            services.AddSingleton<UsbCryptoService>();
            services.AddSingleton<LogSyncService>();
            // Servicios propios de admin
            services.AddSingleton<MonitoringService>();
            //services.AddSingleton<AuthService>();

            var sp = services.BuildServiceProvider();

            /* === Pantalla de login (puede reutilizar la del empleado) === */
            var login = new LoginForm(
                sp.GetRequiredService<ApiClient>(),
                sp.GetRequiredService<UsbCryptoService>(),
                null,
                sp.GetRequiredService<LogSyncService>());

            if (login.ShowDialog() != DialogResult.OK)
                return;  // Login cancelado o fallido

            /* ==== Flujo principal Admin ==== */
            var monService = sp.GetRequiredService<MonitoringService>();
            //var authService = sp.GetRequiredService<AuthService>();

            using var cts = new CancellationTokenSource();
            _ = monService.StartPollingAsync(cts.Token);   // monitoreo en background

            var mainForm = new MainForm(monService, api);

            using var guard = new UsbSessionGuard(async () => await mainForm.LogoutFromUsbRemovalAsync()); ;

            Application.Run(new MainForm(monService, api));
            cts.Cancel();
        }
    }
}
