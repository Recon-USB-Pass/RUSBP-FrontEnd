// RUSBP_Admin/Program.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using RUSBP_Admin.Core.Helpers;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms;
using RUSBP_Admin.Forms.Shared;

namespace RUSBP_Admin
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            /* ───────────── Mutex para evitar 2º instancia ───────────── */
            bool created;
            var mutex = new Mutex(true, "RUSBP_USB_LOCK_AGENT_ADMIN", out created);
            if (!created) return;

            ApplicationConfiguration.Initialize();

            try
            {
                string rpRoot, backendIp;

                /* ───────────── 1. Instalación (una sola vez) ───────────── */
                var settings = SettingsStore.Load();
                if (settings is null)
                {
                    (rpRoot, backendIp) = SetupFirstRunWithUsbRoot();
                    SettingsStore.Save(rpRoot, backendIp);
                }
                else
                {
                    rpRoot = settings.Value.rpRoot;
                    backendIp = settings.Value.backendIp;
                }

                // ↘  RP_root accesible para el resto de la app
                UsbCryptoService.RpRootGlobal = rpRoot;

                /* ───────────── 2. DI container ───────────── */
                var services = new ServiceCollection();
                services.AddSingleton(new ApiClient(backendIp));
                services.AddSingleton<UsbCryptoService>();
                services.AddSingleton<LogSyncService>();
                services.AddSingleton<MonitoringService>();

                using var sp = services.BuildServiceProvider();

                /* ───────────── 3. Login (nuevo LoginForm unificado) ───────────── */
                var login = new LoginForm(
                    sp.GetRequiredService<ApiClient>(),
                    sp.GetRequiredService<UsbCryptoService>(),
                    null,
                    sp.GetRequiredService<LogSyncService>());

                if (login.ShowDialog() != DialogResult.OK)
                    return;                                 // usuario canceló

                /* ───────────── 4. Vista principal + monitor ───────────── */
                var monService = sp.GetRequiredService<MonitoringService>();
                using var cts = new CancellationTokenSource();
                _ = monService.StartPollingAsync(cts.Token); // fire-and-forget

                var mainForm = new MainForm(monService, sp.GetRequiredService<ApiClient>());
                Application.Run(mainForm);
                cts.Cancel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fatal al iniciar la aplicación:\n\n{ex}",
                                "Error crítico",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Flujo de instalación: pide RP_root, desbloquea USB ROOT, valida estructura
        /// y devuelve (rpRoot, backendIp).
        /// </summary>
        private static (string rpRoot, string backendIp) SetupFirstRunWithUsbRoot()
        {
            while (true)
            {
                /* 1️⃣ Detectar algún USB */
                var usb = UsbCryptoService.EnumerateUsbInfos().FirstOrDefault();
                if (usb is null)
                {
                    MessageBox.Show("Conecta el USB ROOT cifrado (rusbp.sys + pki).",
                                    "USB root requerido",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Thread.Sleep(1200); continue;
                }

                string rootPath = usb.Roots.First();     // «F:\»
                string driveLetter = rootPath[..2];         // «F:»

                /* 2️⃣ Pedir RP_root */
                if (!Prompt.ForRecoveryPassword(out string rpRoot))
                {
                    MessageBox.Show("El RecoveryPassword es obligatorio.",
                                    "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                /* 3️⃣ Desbloquear BitLocker */
                if (!CryptoHelper.UnlockBitLockerWithRecoveryPass(driveLetter, rpRoot, out _))
                {
                    MessageBox.Show("No se pudo desbloquear la unidad.",
                                    "BitLocker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                /* 4️⃣ Esperar montaje + validar carpetas */
                string sysDir = Path.Combine(rootPath, "rusbp.sys");
                string pkiDir = Path.Combine(rootPath, "pki");
                int waited = 0;
                while ((!Directory.Exists(sysDir) || !Directory.Exists(pkiDir)) && waited < 18_000)
                {
                    Thread.Sleep(1000); waited += 1000;
                }
                if (!Directory.Exists(sysDir) || !Directory.Exists(pkiDir))
                {
                    MessageBox.Show("Estructura rusbp.sys/pki no encontrada.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                /* 5️⃣ Descifrar .btlk-ip → backendIp */
                string backendIp = CryptoHelper.DecryptBtlkIp(Path.Combine(sysDir, ".btlk-ip"), rpRoot);
                if (string.IsNullOrWhiteSpace(backendIp))
                {
                    MessageBox.Show("No se pudo leer la IP del backend.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                MessageBox.Show($"IP backend detectada: {backendIp}",
                                "Instalación OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return (rpRoot, backendIp.Trim());
            }
        }
    }
}
