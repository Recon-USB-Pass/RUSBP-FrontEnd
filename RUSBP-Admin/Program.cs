using System;
using System.Threading;
using System.Windows.Forms;
using RUSBP_Admin.Core.Services;
using RUSBP_Admin.Forms;

namespace RUSBP_Admin
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var api = ApiClientFactory.Create("http://192.168.1.100:8080");

            var auth = new AuthService(api);
            var mon = new MonitoringService(api);

            using var login = new LoginForm(auth);
            if (login.ShowDialog() != DialogResult.OK) return;

            using var cts = new CancellationTokenSource();
            _ = mon.StartPollingAsync(cts.Token);   // ✔ firma corregida

            Application.Run(new MainForm(mon, auth, api));
            cts.Cancel();
        }
    }
}




/*
using RUSBP_Admin.Forms;

namespace RUSBP_Admin
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            using var login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK)
                Application.Run(new MainForm());

        }
    }
}
*/