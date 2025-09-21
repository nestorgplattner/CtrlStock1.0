using System.Windows;

namespace CapaPresentacionWPF.Servicios
{
    public static class FacturacionService
    {
        public static void MostrarVentanaFacturacion(Window owner)
        {
            var ventana = new VentanaFacturacion();
            ventana.Owner = owner;
            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                var cliente = ventana.ClienteSeleccionado;
                MessageBox.Show($"Facturando a: {cliente.Nombre} ({cliente.CUIT})");
            }
        }
    }
}