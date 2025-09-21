using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;

namespace CapaPresentacionWPF.UserControls
{
    /// <summary>
    /// Lógica de interacción para ucMercadoPago.xaml
    /// </summary>
    public partial class ucMercadoPago : UserControl
    {
        private string connectionString = @"Data Source=Data\stockdb.db;Version=3;";

        public ucMercadoPago()
        {
            InitializeComponent();
            // Cargar los datos de configuración desde la base de datos al iniciar el control
            LoadMercadoPagoCredentials();
            // Inicializar los DatePicker con la fecha actual
            FechaInicioPicker.SelectedDate = DateTime.Today;
            FechaFinPicker.SelectedDate = DateTime.Today;
        }

        /// <summary>
        /// Carga las credenciales de Mercado Pago desde la base de datos.
        /// </summary>
        private void LoadMercadoPagoCredentials()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MpToken, MpStoreId, MpPosId FROM Configuracion WHERE Id = 1";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                AccessTokenTextBox.Text = reader["MpToken"].ToString();
                                StoreIdTextBox.Text = reader["MpStoreId"].ToString();
                                PosIdTextBox.Text = reader["MpPosId"].ToString();
                                StatusMessageTextBlock.Text = "Credenciales cargadas desde la base de datos.";
                            }
                            else
                            {
                                StatusMessageTextBlock.Text = "No se encontraron credenciales en la base de datos.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar las credenciales de Mercado Pago: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusMessageTextBlock.Text = "Error al cargar credenciales.";
                }
            }
        }

        /// <summary>
        /// Traduce el ID del método de pago de la API de Mercado Pago a un string más legible,
        /// incluyendo el nombre de la tarjeta si está disponible.
        /// La nueva lógica prioriza "QR" si la búsqueda es de punto de venta y el pago es con dinero en cuenta.
        /// </summary>
        /// <param name="pago">Objeto PagoMercadoPago que contiene la información del pago.</param>
        /// <param name="isPosSearch">Indica si la consulta fue hecha con Store ID y POS ID.</param>
        /// <returns>El nombre del método de pago (ej. "QR", "Débito (MasterCard)", etc.).</returns>
        private string GetReadablePaymentMethod(PagoMercadoPago pago, bool isPosSearch)
        {
            // Nueva lógica: si la búsqueda es de POS y el pago es con el saldo de la cuenta,
            // lo etiquetamos como QR para reflejar el tipo de comisión.
            if (isPosSearch && pago?.PaymentMethod?.Id == "account_money")
            {
                return "QR";
            }
            // Si no es un pago de POS con dinero en cuenta, se usa la lógica anterior
            // para el resto de los casos.
            else if (pago?.PaymentMethod?.Id == "account_money")
            {
                return "Dinero en Cuenta";
            }
            else if (pago?.PaymentMethod?.Type == "credit_card" || pago?.PaymentMethod?.Type == "debit_card" || pago?.PaymentMethod?.Type == "prepaid_card")
            {
                string tipoTarjeta = pago.PaymentMethod?.Type == "debit_card" ? "Débito" : pago.PaymentMethod?.Type == "credit_card" ? "Crédito" : "Prepagada";
                string marcaTarjeta = pago.PaymentMethod?.Id;

                return $"{tipoTarjeta} ({marcaTarjeta?.Replace("_card", "").ToUpper()})";
            }
            else
            {
                // Fallback para cualquier otro tipo de pago no contemplado.
                return pago?.PaymentMethod?.Id ?? "Desconocido";
            }
        }

        /// <summary>
        /// Método principal para consultar la API de Mercado Pago, ahora con consultas diarias y paginación.
        /// </summary>
        private async Task ConsultarMercadoPago(DateTime fechaInicio, DateTime fechaFin, bool isPosSearch)
        {
            // Mover la lógica de deshabilitar/habilitar los botones a este método
            // para asegurar que siempre se ejecuta.
            // Deshabilitar los botones de consulta mientras la operación está en curso
            ConsultarButton.IsEnabled = false;
            ConsultarGeneralButton.IsEnabled = false;
            ConsultarHoyButton.IsEnabled = false;

            // Limpiar la visualización y mostrar un mensaje de estado
            PagosDataGrid.ItemsSource = null;
            JsonResponseTextBox.Clear();
            StatusMessageTextBlock.Text = "Iniciando consulta...";

            // Agregar un breve "respiro" para que la UI pueda refrescarse visualmente
            // y mostrar los botones deshabilitados y el mensaje de estado.
            await Task.Yield();

            string accessToken = AccessTokenTextBox.Text.Trim();

            if (string.IsNullOrEmpty(accessToken))
            {
                MessageBox.Show("Por favor, completa el campo Access Token. Puedes hacerlo en la sección de Configuración.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusMessageTextBlock.Text = "Error: Falta Access Token.";
                // El bloque finally se encargará de re-habilitar los botones.
                return;
            }

            string baseUrl = "https://api.mercadopago.com/v1/payments/search";
            if (isPosSearch)
            {
                string storeId = StoreIdTextBox.Text.Trim();
                string posId = PosIdTextBox.Text.Trim();

                if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(posId))
                {
                    MessageBox.Show("Por favor, completa los campos Store ID y POS ID para esta consulta. Puedes hacerlo en la sección de Configuración.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusMessageTextBlock.Text = "Error: Faltan Store ID o POS ID.";
                    // El bloque finally se encargará de re-habilitar los botones.
                    return;
                }
                baseUrl += $"?store_id={storeId}&pos_id={posId}";
            }

            List<PagoMercadoPago> allPagos = new List<PagoMercadoPago>();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    for (DateTime day = fechaInicio.Date; day <= fechaFin.Date; day = day.AddDays(1))
                    {
                        StatusMessageTextBlock.Text = $"Consultando transacciones del día {day.ToString("dd/MM/yyyy")}...";

                        DateTime dayStart = day.Date;
                        DateTime dayEnd = day.Date + new TimeSpan(23, 59, 59);
                        string dayStartFormat = dayStart.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string dayEndFormat = dayEnd.ToString("yyyy-MM-ddTHH:mm:ssZ");

                        int offset = 0;
                        const int limit = 50;
                        int totalResults = 0;

                        do
                        {
                            string paginatedUrl = baseUrl + (baseUrl.Contains("?") ? "&" : "?") +
                                $"begin_date={dayStartFormat}&end_date={dayEndFormat}&offset={offset}&limit={limit}";

                            HttpResponseMessage response = await client.GetAsync(paginatedUrl);
                            string jsonResponse = await response.Content.ReadAsStringAsync();

                            if (offset == 0 && day == fechaInicio.Date)
                            {
                                JsonResponseTextBox.Text = jsonResponse;
                            }

                            if (response.IsSuccessStatusCode)
                            {
                                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                                if (apiResponse?.Results != null && apiResponse.Results.Any())
                                {
                                    allPagos.AddRange(apiResponse.Results);
                                    totalResults = apiResponse.Paging.Total;
                                    offset += limit;
                                    // Actualizar el estado con el progreso
                                    StatusMessageTextBlock.Text = $"Cargando... {allPagos.Count} de {totalResults} transacciones.";
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Error al consultar la API para el día {day.ToString("dd/MM/yyyy")}: {response.StatusCode} - {response.ReasonPhrase}\nDetalle: {jsonResponse}", "Error de API", MessageBoxButton.OK, MessageBoxImage.Error);
                                StatusMessageTextBlock.Text = "Error en la consulta de la API.";
                                return;
                            }
                        } while (offset < totalResults);
                    }

                    StatusMessageTextBlock.Text = $"Consulta completada. Total de {allPagos.Count} resultados.";

                    if (allPagos.Any())
                    {
                        var pagosParaMostrar = allPagos
                            .OrderByDescending(p => p.DateCreated) // Ordenado de forma descendente (lo más nuevo primero)
                            .Select(pago => new PagoDisplayModel
                            {
                                Id = pago.Id,
                                Status = pago.Status,
                                StatusDetail = pago.StatusDetail,
                                Description = pago.Description,
                                DateCreated = pago.DateCreated,
                                MontoTotal = pago.TransactionAmount,
                                MontoNeto = pago.TransactionDetails?.NetReceivedAmount ?? 0,
                                ComisionMP = pago.ChargesDetails?.FirstOrDefault(c => c.Name == "mercadopago_fee")?.Amounts.Original ?? 0,
                                Impuestos = pago.ChargesDetails?.Where(c => c.Type == "tax").Sum(c => c.Amounts.Original) ?? 0,
                                MetodoDePago = GetReadablePaymentMethod(pago, isPosSearch),
                                PorcentajeComision = pago.TransactionAmount > 0 ? ((pago.ChargesDetails?.FirstOrDefault(c => c.Name == "mercadopago_fee")?.Amounts.Original ?? 0) + (pago.ChargesDetails?.Where(c => c.Type == "tax").Sum(c => c.Amounts.Original) ?? 0)) / pago.TransactionAmount : 0
                            }).ToList();

                        ICollectionView view = CollectionViewSource.GetDefaultView(pagosParaMostrar);
                        if (view != null && !view.GroupDescriptions.Any())
                        {
                            view.GroupDescriptions.Add(new PropertyGroupDescription("DateCreated", new DateConverter()));
                        }

                        PagosDataGrid.ItemsSource = view;
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron transacciones en el rango de fechas seleccionado.", "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                        StatusMessageTextBlock.Text = "No se encontraron resultados.";
                    }
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error al procesar el JSON de la API: {ex.Message}", "Error de Deserialización", MessageBoxButton.OK, MessageBoxImage.Error);
                JsonResponseTextBox.Text = $"Error de JSON: {ex.Message}\n\nJSON recibido:\n{JsonResponseTextBox.Text}";
                StatusMessageTextBlock.Text = "Error de JSON.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                JsonResponseTextBox.Text = $"Error inesperado: {ex.Message}";
                StatusMessageTextBlock.Text = "Error inesperado.";
            }
            finally
            {
                // Habilitar los botones de consulta una vez que la operación ha finalizado, sin importar el resultado
                ConsultarButton.IsEnabled = true;
                ConsultarGeneralButton.IsEnabled = true;
                ConsultarHoyButton.IsEnabled = true;
            }
        }

        private async void ConsultarButton_Click(object sender, RoutedEventArgs e)
        {
            if (FechaInicioPicker.SelectedDate == null || FechaFinPicker.SelectedDate == null)
            {
                MessageBox.Show("Por favor, selecciona un rango de fechas válido.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaInicio = FechaInicioPicker.SelectedDate.Value.Date;
            DateTime fechaFin = FechaFinPicker.SelectedDate.Value.Date;

            await ConsultarMercadoPago(fechaInicio, fechaFin, isPosSearch: true);
        }

        private async void ConsultarGeneralButton_Click(object sender, RoutedEventArgs e)
        {
            if (FechaInicioPicker.SelectedDate == null || FechaFinPicker.SelectedDate == null)
            {
                MessageBox.Show("Por favor, selecciona un rango de fechas válido.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaInicio = FechaInicioPicker.SelectedDate.Value.Date;
            DateTime fechaFin = FechaFinPicker.SelectedDate.Value.Date;

            await ConsultarMercadoPago(fechaInicio, fechaFin, isPosSearch: false);
        }

        private async void ConsultarHoyButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today;
            FechaInicioPicker.SelectedDate = today;
            FechaFinPicker.SelectedDate = today;

            await ConsultarMercadoPago(today, today, isPosSearch: false);
        }

        private void LimpiarButton_Click(object sender, RoutedEventArgs e)
        {
            AccessTokenTextBox.Clear();
            StoreIdTextBox.Clear();
            PosIdTextBox.Clear();
            FechaInicioPicker.SelectedDate = null;
            FechaFinPicker.SelectedDate = null;
            PagosDataGrid.ItemsSource = null;
            JsonResponseTextBox.Clear();
            StatusMessageTextBlock.Text = string.Empty;
        }

        // DTO (Data Transfer Object) para el DataGrid
        public class PagoDisplayModel
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string StatusDetail { get; set; }
            public string Description { get; set; }
            public DateTime DateCreated { get; set; }
            public decimal MontoTotal { get; set; }
            public decimal MontoNeto { get; set; }
            public decimal ComisionMP { get; set; }
            public decimal Impuestos { get; set; }
            public string MetodoDePago { get; set; }
            public decimal PorcentajeComision { get; set; }
        }

        // CLASES DEL MODELO DE DATOS para deserializar el JSON completo
        public class ApiResponse
        {
            [JsonProperty("results")]
            public List<PagoMercadoPago> Results { get; set; }

            [JsonProperty("paging")]
            public Paging Paging { get; set; }
        }

        public class Paging
        {
            [JsonProperty("total")]
            public int Total { get; set; }

            [JsonProperty("offset")]
            public int Offset { get; set; }

            [JsonProperty("limit")]
            public int Limit { get; set; }
        }

        public class PagoMercadoPago
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("status_detail")]
            public string StatusDetail { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("transaction_amount")]
            public decimal TransactionAmount { get; set; }

            [JsonProperty("date_created")]
            public DateTime DateCreated { get; set; }

            [JsonProperty("payment_method")]
            public PaymentMethod PaymentMethod { get; set; }

            [JsonProperty("payment_type_id")]
            public string PaymentTypeId { get; set; }

            [JsonProperty("transaction_details")]
            public TransactionDetails TransactionDetails { get; set; }

            [JsonProperty("charges_details")]
            public List<ChargeDetail> ChargesDetails { get; set; }
        }

        public class PaymentMethod
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
        }

        public class TransactionDetails
        {
            [JsonProperty("net_received_amount")]
            public decimal? NetReceivedAmount { get; set; }
        }

        public class ChargeDetail
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("amounts")]
            public ChargeAmounts Amounts { get; set; }
        }

        public class ChargeAmounts
        {
            [JsonProperty("original")]
            public decimal Original { get; set; }
        }

        // Conversor para agrupar por la fecha sin la hora.
        public class DateConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is DateTime)
                {
                    return ((DateTime)value).Date;
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
