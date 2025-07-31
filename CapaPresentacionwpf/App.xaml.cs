using CapaPresentacionWPF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace CapaPresentacionWPF
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var splash = new SplashScreen();
            splash.Show();

            await Task.Delay(2000);

            splash.Close();

            var login = new frmLogin();
            bool? result = login.ShowDialog();

            if (result == true)
            {
                // Login exitoso: abrimos ventana principal
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // Login cancelado o fallido, cerramos app
                Shutdown();
            }
        }
    }
}