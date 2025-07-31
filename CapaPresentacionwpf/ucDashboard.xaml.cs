using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Data.SQLite;
using System.Collections.ObjectModel; // Necesario para ObservableCollection (si la usas en el XAML)
using System.Globalization; // Necesario para CultureInfo para formato de moneda

namespace CapaPresentacionWPF.UserControls
{
    /// <summary>
    /// Lógica de interacción para ucDashboard.xaml
    /// </summary>
    public partial class ucDashboard : UserControl
    {
        private string connectionString = @"Data Source=Data\stockdb.db;Version=3;";

        public ucDashboard()
        {
            InitializeComponent();
            LoadDashboardData(); // Llama a un método para cargar los datos de tu dashboard
        }

        private void LoadDashboardData()
        {
            decimal totalHoy = 0m;
            decimal totalEfectivo = 0m;
            decimal totalOtros = 0m;
            decimal totalFlor = 0m;

            // Cambiado a ObservableCollection para mejor binding si el UI se actualiza dinámicamente
            // Si solo lo cargas una vez, List<ChartDataItem> también es válido.
            ObservableCollection<ChartDataItem> ventasPorFormaPago = new ObservableCollection<ChartDataItem>();
            List<string> stockAlerts = new List<string>();
            decimal ticketPromedioHoy = 0m;

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Consulta para los totales superiores
                using (var cmd = new SQLiteCommand(@"
                    SELECT
                        SUM(CASE WHEN strftime('%Y-%m-%d', Fecha) = strftime('%Y-%m-%d', 'now', 'localtime') THEN Total ELSE 0 END) AS TotalHoy,
                        SUM(CASE WHEN fp.Plataforma = 'Efectivo' THEN v.Total ELSE 0 END) AS TotalEfectivo,
                        SUM(CASE WHEN fp.Plataforma <> 'Efectivo' AND v.EsFlor = 0 THEN v.Total ELSE 0 END) AS TotalOtros,
                        SUM(v.TotalFlor) AS TotalFlor
                    FROM Ventas v
                    JOIN FormaPago fp ON v.IdFormaPago = fp.IdFormaPago;", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalHoy = reader["TotalHoy"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("TotalHoy")) : 0m;
                            totalEfectivo = reader["TotalEfectivo"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("TotalEfectivo")) : 0m;
                            totalOtros = reader["TotalOtros"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("TotalOtros")) : 0m;
                            totalFlor = reader["TotalFlor"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("TotalFlor")) : 0m;
                        }
                    }
                }

                // Consulta para las Ventas por Forma de Pago (Hoy)
                var tempSalesByPaymentMethod = new Dictionary<string, decimal>();
                decimal totalSalesTodayForPercentage = 0m; // Acumulador para el total del día

                using (var cmd = new SQLiteCommand(@"
                    SELECT
                        fp.Plataforma || ' - ' || fp.MedioPago AS FormaPago,
                        SUM(v.Total) AS TotalAmount
                    FROM Ventas v
                    JOIN FormaPago fp ON v.IdFormaPago = fp.IdFormaPago
                    WHERE strftime('%Y-%m-%d', v.Fecha) = strftime('%Y-%m-%d', 'now', 'localtime') AND v.EsFlor = 0
                    GROUP BY fp.Plataforma, fp.MedioPago;", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string formaPago = reader["FormaPago"].ToString();
                            decimal amount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"));
                            tempSalesByPaymentMethod[formaPago] = amount;
                            totalSalesTodayForPercentage += amount;
                        }
                    }
                }

                // Agrega las ventas de "flor" a los datos del gráfico
                using (var cmdFlor = new SQLiteCommand(@"
                    SELECT SUM(TotalFlor) AS TotalFlorToday
                    FROM Ventas
                    WHERE strftime('%Y-%m-%d', Fecha) = strftime('%Y-%m-%d', 'now', 'localtime');", conn))
                {
                    object result = cmdFlor.ExecuteScalar();
                    decimal totalFlorToday = (result != DBNull.Value) ? Convert.ToDecimal(result) : 0m;
                    if (totalFlorToday > 0)
                    {
                        // FIX CS1061: Reemplazamos GetValueOrDefault
                        if (tempSalesByPaymentMethod.ContainsKey("Flor"))
                        {
                            tempSalesByPaymentMethod["Flor"] += totalFlorToday;
                        }
                        else
                        {
                            tempSalesByPaymentMethod.Add("Flor", totalFlorToday);
                        }
                        totalSalesTodayForPercentage += totalFlorToday; // Asegurarse de que el total incluye Flor
                    }
                }

                foreach (var entry in tempSalesByPaymentMethod)
                {
                    decimal percentage = (totalSalesTodayForPercentage > 0) ? (entry.Value / totalSalesTodayForPercentage) * 100m : 0m;
                    ventasPorFormaPago.Add(new ChartDataItem
                    {
                        Label = entry.Key,
                        Value = entry.Value,
                        Percentage = (double)percentage // FIX CS0266: Conversión explícita a double
                    });
                }

                // Consulta para el Ticket Promedio Hoy
                using (var cmd = new SQLiteCommand(@"
                    SELECT AVG(Total) FROM Ventas
                    WHERE strftime('%Y-%m-%d', Fecha) = strftime('%Y-%m-%d', 'now', 'localtime');", conn))
                {
                    object result = cmd.ExecuteScalar();
                    ticketPromedioHoy = (result != DBNull.Value) ? Convert.ToDecimal(result) : 0m;
                }

                // Consulta para las Alertas de Stock Bajo
                // Asegúrate de que tu tabla se llama 'Productos' (plural) y no 'Producto' (singular)
                // Y que tiene una columna 'MinStockAlert' o define un umbral fijo aquí.
                int umbralStockBajo = 5; // Define tu umbral de stock bajo aquí
                using (var cmd = new SQLiteCommand("SELECT Nombre, Stock FROM Producto WHERE Stock <= @Umbral LIMIT 5;", conn))
                {
                    cmd.Parameters.AddWithValue("@Umbral", umbralStockBajo); // Agrega el parámetro
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stockAlerts.Add($"{reader["Nombre"]} (Stock: {reader["Stock"]})");
                        }
                    }
                }
            }

            // Actualizar elementos de la interfaz de usuario
            lblTotalHoy.Text = totalHoy.ToString("C", System.Globalization.CultureInfo.CurrentCulture);
            lblTotalEfectivo.Text = totalEfectivo.ToString("C", System.Globalization.CultureInfo.CurrentCulture);
            lblTotalOtros.Text = totalOtros.ToString("C", System.Globalization.CultureInfo.CurrentCulture);
            lblTotalFlor.Text = totalFlor.ToString("C", System.Globalization.CultureInfo.CurrentCulture);
            lblTicketPromedioHoy.Text = ticketPromedioHoy.ToString("C", System.Globalization.CultureInfo.CurrentCulture);

            icVentasPorFormaPagoHoy.ItemsSource = ventasPorFormaPago;

            if (stockAlerts.Any())
            {
                icStockAlerts.ItemsSource = stockAlerts;
                lblNoStockAlerts.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                icStockAlerts.ItemsSource = null;
                lblNoStockAlerts.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }

    // Clase auxiliar para los datos del gráfico
    public class ChartDataItem
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
        public double Percentage { get; set; } // Cambiado a double para compatibilidad
    }
}