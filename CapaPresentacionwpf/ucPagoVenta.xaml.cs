using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacionwpf
{

    // Clase para representar un pago parcial
    public class PagoParcial
    {
        public string Plataforma { get; set; } = "Efectivo";  // Por defecto
        public string MedioPago { get; set; } = "Contado";    // Por defecto
        public decimal Monto { get; set; } = 0;
        public int Cuotas { get; set; } = 1;
    }

    public partial class ucPagoVenta : UserControl
    {
        // Lista observable para que el DataGrid actualice cuando cambia
        private ObservableCollection<PagoParcial> listaPagos = new ObservableCollection<PagoParcial>();

        // Valores para dropdowns fijos, luego podes cargar desde DB si queres
        private readonly string[] plataformas = new[] { "Efectivo", "MercadoPago", "CuentaDNI", "Tarjeta" };
        private readonly string[] mediosPago = new[] { "Contado", "Débito", "Crédito", "Transferencia" };

        public ucPagoVenta()
        {
            InitializeComponent();

            dgPagos.ItemsSource = listaPagos;

            // Agregar el primer pago por defecto para que la lista no esté vacía
            listaPagos.Add(new PagoParcial { Plataforma = "Efectivo", MedioPago = "Contado", Monto = 0, Cuotas = 1 });

            // Inicializar combos del DataGrid (ComboBoxColumn no tiene ItemsSource directamente, se setea manualmente)
            SetupComboBoxColumns();

            ActualizarResumen();
        }

        private void SetupComboBoxColumns()
        {
            // Setear ItemsSource para columnas ComboBox (plataforma y medio de pago)
            var colPlataforma = dgPagos.Columns[0] as DataGridComboBoxColumn;
            var colMedioPago = dgPagos.Columns[1] as DataGridComboBoxColumn;

            if (colPlataforma != null) colPlataforma.ItemsSource = plataformas;
            if (colMedioPago != null) colMedioPago.ItemsSource = mediosPago;
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            listaPagos.Add(new PagoParcial { Plataforma = plataformas[0], MedioPago = mediosPago[0], Monto = 0, Cuotas = 1 });
        }

        private void BtnQuitar_Click(object sender, RoutedEventArgs e)
        {
            if (dgPagos.SelectedItem is PagoParcial pago)
            {
                listaPagos.Remove(pago);
                ActualizarResumen();
            }
            else
            {
                MessageBox.Show("Seleccione un medio de pago para quitar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dgPagos_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Se ejecuta cuando editás una celda, actualizamos resumen al terminar
            this.Dispatcher.InvokeAsync(() =>
            {
                ActualizarResumen();
            });
        }

        private void ActualizarResumen()
        {
            decimal total = listaPagos.Sum(p => p.Monto);
            decimal totalEfectivo = listaPagos
                .Where(p => p.Plataforma == "Efectivo" || p.MedioPago == "Transferencia")
                .Sum(p => p.Monto);

            txtResumen.Text = $"Total Pagado: {total:C2} | Total con Descuento Aplicable (Efectivo/Transferencia): {totalEfectivo:C2}";
        }

        // Método para obtener el listado actual para procesar o guardar en venta
        public ObservableCollection<PagoParcial> ObtenerPagos()
        {
            return listaPagos;
        }




    }
}