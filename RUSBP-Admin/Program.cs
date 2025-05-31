using Microsoft.Extensions.DependencyInjection;
using RUSBP_Admin.Core.Helpers;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms;
using RUSBP_Admin.Forms.Shared;
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
            //using var guard = new LoginForm. UsbSessionGuard(async () => await mainForm.LogoutFromUsbRemovalAsync());

            Application.Run(mainForm);
            cts.Cancel();
        }

        /// <summary>
        /// Lógica inicial robusta para setup de USB root: detecta unidad, pide RP_root, desbloquea, valida estructura.
        /// </summary>
        private static (string rpRoot, string backendIp) SetupFirstRunWithUsbRoot()
        {
            DriveInfo usbDrive = null;

            // 1. Esperar a que haya al menos una unidad USB conectada (sin requerir IsReady)
            while (usbDrive == null)
            {
                var candidates = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Removable)
                    .ToList();

                if (candidates.Count == 0)
                {
                    MessageBox.Show(
                        "Conecta un USB Root cifrado.",
                        "USB root requerido", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Thread.Sleep(1200);
                    continue;
                }
                // Si hay más de uno, permite elegir
                usbDrive = (candidates.Count == 1)
                    ? candidates[0]
                    : ShowSelectDriveDialog(candidates);
            }

            // 2. Pedir RecoveryPassword
            if (!Prompt.ForRecoveryPassword(out string recoveryPassword))
            {
                MessageBox.Show("El RecoveryPassword es obligatorio para instalar.", "Operación cancelada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }

            // 3. Intentar desbloquear con BitLocker (usa la letra de la unidad, p.ej. "F:")
            string driveLetter = usbDrive.Name.TrimEnd('\\').TrimEnd(':') + ":";
            string cmd = $"manage-bde -unlock {driveLetter} -RecoveryPassword {recoveryPassword}";//manage-bde.exe
            Console.WriteLine("BITLOCKER CMD: " + cmd);
            Debug.WriteLine("BITLOCKER CMD: " + cmd);

            if (!CryptoHelper.UnlockBitLockerWithRecoveryPass(driveLetter, recoveryPassword))
            {
                MessageBox.Show("No se pudo desbloquear la unidad con ese RecoveryPassword.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            // 4. Esperar a que IsReady == true y validar estructura interna
            int waited = 0, maxWait = 18000;
            while ((!usbDrive.IsReady || !Directory.Exists(Path.Combine(usbDrive.RootDirectory.FullName, "rusbp.sys")))
                    && waited < maxWait)
            {
                Thread.Sleep(1000);
                usbDrive = DriveInfo.GetDrives()
                    .FirstOrDefault(d => d.Name == usbDrive.Name);
                waited += 1000;
            }

            if (!usbDrive.IsReady || !Directory.Exists(Path.Combine(usbDrive.RootDirectory.FullName, "rusbp.sys")))
            {
                MessageBox.Show("No se detecta estructura esperada tras desbloqueo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            // 5. Leer IP del backend desde .btlk-ip y validar .btlk
            string sysDir = Path.Combine(usbDrive.RootDirectory.FullName, "rusbp.sys");
            string btlkIpPath = Path.Combine(sysDir, ".btlk-ip");
            string backendIp = CryptoHelper.DecryptBtlkIp(btlkIpPath, recoveryPassword);

            string btlkPath = Path.Combine(sysDir, ".btlk");
            if (!File.Exists(btlkPath))
            {
                MessageBox.Show("No se encontró el archivo de validación .btlk en el USB Root.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            // Validar .btlk con el recoveryPassword
            try
            {
                var check = CryptoHelper.DecryptBtlk(btlkPath, recoveryPassword);
                if (check != recoveryPassword)
                    throw new Exception("La clave ingresada no valida el .btlk del USB Root.");
            }
            catch
            {
                MessageBox.Show("La clave RP_root no valida el USB Root. Asegúrate de ingresar la clave correcta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            if (string.IsNullOrWhiteSpace(backendIp))
            {
                MessageBox.Show("No se pudo leer la IP del backend desde el USB root.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            MessageBox.Show($"IP del backend detectada: {backendIp}", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return (recoveryPassword, backendIp.Trim());
        }



        /// <summary>
        /// Dialog simple para elegir unidad USB si hay varias conectadas
        /// </summary>
        private static DriveInfo ShowSelectDriveDialog(System.Collections.Generic.List<DriveInfo> drives)
        {
            // Puedes personalizar el diálogo; aquí uno mínimo.
            string msg = "Selecciona la unidad USB Root:\n\n";
            for (int i = 0; i < drives.Count; i++)
            {
                msg += $"{i + 1}) {drives[i].Name} ({drives[i].VolumeLabel})\n";
            }
            string? input = Microsoft.VisualBasic.Interaction.InputBox(msg, "Seleccionar unidad", "1");
            if (int.TryParse(input, out int sel) && sel >= 1 && sel <= drives.Count)
                return drives[sel - 1];
            return null;
        }

        

    }
}
