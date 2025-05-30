using Microsoft.Extensions.DependencyInjection;
using RUSBP_Admin.Core.Helpers;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RUSBP_Admin
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(true, "RUSBP_USB_LOCK_AGENT_ADMIN", out bool createdNew);
            if (!createdNew) return;

            ApplicationConfiguration.Initialize();

            string rpRoot, backendIp;

            // ① Verifica si es primera ejecución
            var settings = SettingsStore.Load();
            if (settings == null)
            {
                (rpRoot, backendIp) = SetupFirstRunWithUsbRoot();
                SettingsStore.Save(rpRoot, backendIp);
            }
            else
            {
                rpRoot = settings.Value.rpRoot;
                backendIp = settings.Value.backendIp;
            }

            // ② Inyección de dependencias
            var services = new ServiceCollection();
            services.AddSingleton(new ApiClient(backendIp));
            services.AddSingleton<UsbCryptoService>();
            services.AddSingleton<LogSyncService>();
            services.AddSingleton<MonitoringService>();

            var sp = services.BuildServiceProvider();

            // ③ Login
            var login = new LoginForm(
                sp.GetRequiredService<ApiClient>(),
                sp.GetRequiredService<UsbCryptoService>(),
                null,
                sp.GetRequiredService<LogSyncService>());

            if (login.ShowDialog() != DialogResult.OK)
                return;

            // ④ Vista principal + monitoreo
            var monService = sp.GetRequiredService<MonitoringService>();
            var api = sp.GetRequiredService<ApiClient>();
            using var cts = new CancellationTokenSource();
            _ = monService.StartPollingAsync(cts.Token);

            var mainForm = new MainForm(monService, api);
            using var guard = new LoginForm.UsbSessionGuard(async () => await mainForm.LogoutFromUsbRemovalAsync());

            Application.Run(mainForm);
            cts.Cancel();
        }

        /// <summary>
        /// Flujo inicial para detectar USB Root y obtener IP + rpRoot.
        /// </summary>
        private static (string rpRoot, string backendIp) SetupFirstRunWithUsbRoot()
        {
            while (true)
            {
                var usbList = UsbCryptoService.EnumerateUsbInfos();
                if (usbList.Count == 0)
                {
                    MessageBox.Show("Conecta el USB Root cifrado (con rusbp.sys y pki).",
                                    "USB root requerido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Thread.Sleep(1200);
                    continue;
                }

                var root = usbList.First().Roots.First();
                var driveLetter = root.Substring(0, 2);

                if (!Forms.Prompt.ForRecoveryPassword(out string recoveryPassword))
                {
                    MessageBox.Show("Se requiere RecoveryPassword para continuar.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                if (!UnlockBitLockerWithRecoveryPass(driveLetter, recoveryPassword))
                {
                    MessageBox.Show("No se pudo desbloquear la unidad con ese RecoveryPassword.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                string sysDir = Path.Combine(root, "rusbp.sys");
                string pkiDir = Path.Combine(root, "pki");
                int waited = 0, maxWait = 18000;
                while ((!Directory.Exists(sysDir) || !Directory.Exists(pkiDir)) && waited < maxWait)
                {
                    Thread.Sleep(1000);
                    waited += 1000;
                }

                if (!Directory.Exists(sysDir) || !Directory.Exists(pkiDir))
                {
                    MessageBox.Show("No se detecta estructura esperada tras desbloqueo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                string btlkIpPath = Path.Combine(sysDir, ".btlk-ip");
                string backendIp = CryptoHelper.DecryptBtlkIp(btlkIpPath, recoveryPassword);

                if (string.IsNullOrWhiteSpace(backendIp))
                {
                    MessageBox.Show("No se pudo leer la IP del backend desde el USB root.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                MessageBox.Show($"IP del backend detectada: {backendIp}", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return (recoveryPassword, backendIp.Trim());
            }
        }
        public static bool UnlockBitLockerWithRecoveryPass(string driveLetter, string recoveryPassword)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "manage-bde.exe",
                    Arguments = $"-unlock {driveLetter}: -RecoveryPassword {recoveryPassword}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = System.Diagnostics.Process.Start(psi);
                p.WaitForExit();
                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }


    }
}
