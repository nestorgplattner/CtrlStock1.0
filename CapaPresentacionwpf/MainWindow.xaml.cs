using CapaPresentacionWPF.Model;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CapaPresentacionWPF.UserControls;
using CapaPresentacionWPF.Data; // Asegúrate de que esta referencia sea correcta si la usas

namespace CapaPresentacionWPF
{
    public partial class MainWindow : Window
    {
        public string FechaActual { get; set; }
        private Button botonActivo;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            FechaActual = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            // --- CAMBIO AQUÍ: Cargar ucDashboard por defecto ---
            // Primero, busca el botón de "Dashboard" por su Tag
            Button dashboardButton = null;
            foreach (var child in MenuStackPanel.Children)
            {
                if (child is Button btn && btn.Tag != null && btn.Tag.ToString() == "Dashboard")
                {
                    dashboardButton = btn;
                    break;
                }
            }

            if (dashboardButton != null)
            {
                ActivarBoton(dashboardButton);
                CargarContenido(dashboardButton.Tag.ToString());
            }
            else
            {
                // Si por alguna razón no se encuentra el botón de Dashboard,
                // vuelve al comportamiento por defecto (primer botón)
                if (MenuStackPanel.Children.Count > 0 && MenuStackPanel.Children[0] is Button firstBtn)
                {
                    ActivarBoton(firstBtn);
                    CargarContenido(firstBtn.Tag.ToString());
                }
            }
            // --- FIN DEL CAMBIO ---

            // Actualizar fecha cada minuto (opcional)
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += (s, e) => FechaActual = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            timer.Start();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                ActivarBoton(btn);
                CargarContenido(btn.Tag.ToString());
            }
        }

        private void ActivarBoton(Button btn)
        {
            if (botonActivo != null)
            {
                botonActivo.Style = (Style)FindResource("SidebarButtonStyle");
            }
            botonActivo = btn;
            botonActivo.Style = (Style)FindResource("SidebarButtonActiveStyle");
        }

        private void CargarContenido(string seccion)
        {
            switch (seccion)
            {
                case "Dashboard":
                    ContentArea.Content = new UserControls.ucDashboard();
                    TituloSeccion.Text = "Dashboard Resumen";
                    break;
                case "Productos":
                    ContentArea.Content = new UserControls.ucProducto(); // Asegúrate de tener esta clase ucProducto
                    TituloSeccion.Text = "Gestión Productos";
                    break;
                case "Ventas":
                    ContentArea.Content = new UserControls.ucVenta(); // Asegúrate de tener esta clase ucVenta
                    TituloSeccion.Text = "Sistema Ventas";
                    break;
                case "Registros":
                    ContentArea.Content = new UserControls.ucRegistros();
                    TituloSeccion.Text = "Registros de Ventas";
                    break;
                default:
                    ContentArea.Content = null;
                    TituloSeccion.Text = "";
                    break;
            }
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Deseas salir?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        // Métodos para animar la barra lateral (si tienes el botón para activarlos)
        private void MostrarSidebar()
        {
            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            SidebarTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private void OcultarSidebar()
        {
            var anim = new DoubleAnimation(-256, TimeSpan.FromMilliseconds(300));
            SidebarTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }
    }
}