using CapaPresentacionWPF.Data;
using CapaPresentacionWPF.Model;
using CapaPresentacionWPF.Servicios;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CapaPresentacionWPF.UserControls
{
    public partial class ucVenta : UserControl
    {
        private List<Producto> productosDisponibles = new List<Producto>();
        private List<ItemCarrito> carrito = new List<ItemCarrito>();
        private List<FormaPagoSimple> formasPagoDisponibles = new List<FormaPagoSimple>();
        private List<PagoTemporal> pagosRealizados = new List<PagoTemporal>();

        private readonly string connectionString = @"Data Source=Data\stockdb.db;Version=3;";
        private decimal porcentajeDescuento = 0;
        private string tipoRapidoSeleccionado = null;
        private int idFormaPagoSeleccionada = 0; // Para el botón de forma de pago seleccionado

        #region Inicialización y carga

        public ucVenta()
        {
            InitializeComponent();
            CargarDatosIniciales();
        }

        private void CargarDatosIniciales()
        {
            CargarProductos();
            CargarFormasPago();
            RefrescarUI();
        }

        private void CargarProductos()
        {
            productosDisponibles = new ProductoData(connectionString).ObtenerProductos();
            lstProductos.ItemsSource = productosDisponibles;
        }

        private void CargarFormasPago()
        {
            formasPagoDisponibles = new FormaPagoData(connectionString).ObtenerFormasPago();    
        }

        #endregion

        #region Manejo de productos

        private void lstProductos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstProductos.SelectedItem is Producto producto && producto.Stock > 0)
            {
                CarritoService.AgregarProductoAlCarrito(carrito, producto);
                RefrescarUI();
            }
        }

        private void EliminarItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ItemCarrito item)
            {
                CarritoService.EliminarItemDelCarrito(carrito, productosDisponibles, item);
                RefrescarUI();
            }
        }

        #endregion

        #region Manejo de pagos

        private void btnSeleccionarFormaPago_Click(object sender, RoutedEventArgs e)
        {
            // Resetear estilo de todos los botones de forma de pago
            if (btnEfectivo != null) btnEfectivo.Background = Brushes.LightGray;
            if (btnTransferencia != null) btnTransferencia.Background = Brushes.LightGray;
            if (btnMpTarjetas != null) btnMpTarjetas.Background = Brushes.LightGray;

            if (sender is Button btn)
            {
                btn.Background = Brushes.LightGreen;
                string nombreBoton = btn.Content.ToString();
                FormaPagoSimple formaPagoSeleccionadaDb = null;

                switch (nombreBoton)
                {
                    case "EFECTIVO":
                        formaPagoSeleccionadaDb = formasPagoDisponibles.FirstOrDefault(fp => fp.Plataforma == "Contado" && fp.MedioPago == "Contado");
                        break;
                    case "TRANSFERENCIA":
                        formaPagoSeleccionadaDb = formasPagoDisponibles.FirstOrDefault(fp => fp.Plataforma == "Transferencia" && fp.MedioPago == "Transferencia");
                        break;
                    case "MP / TARJETAS":
                        formaPagoSeleccionadaDb = formasPagoDisponibles.FirstOrDefault(fp => fp.Plataforma == "QR" && fp.MedioPago == "QR");
                        if (formaPagoSeleccionadaDb == null)
                        {
                            formaPagoSeleccionadaDb = formasPagoDisponibles.FirstOrDefault(fp => fp.Plataforma.Contains("Tarjeta") || fp.MedioPago.Contains("Tarjeta"));
                        }
                        break;
                    default:
                        break;
                }

                if (formaPagoSeleccionadaDb != null)
                {
                    idFormaPagoSeleccionada = formaPagoSeleccionadaDb.IdFormaPago;

                    // Al seleccionar una forma de pago, siempre mostramos el campo de monto
                    // y lo precargamos con el monto pendiente actual.
                    decimal totalVenta = 0;
                    if (!decimal.TryParse(lblTotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out totalVenta))
                    {
                        MessageBox.Show("Total de la venta inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    decimal totalPagadoActual = pagosRealizados.Sum(p => p.Monto);
                    decimal montoPendiente = totalVenta - totalPagadoActual;

                    gridMontoPago.Visibility = Visibility.Visible;
                    txtMontoPago.Text = montoPendiente.ToString("N2", CultureInfo.InvariantCulture);

                    // Si el monto pendiente es 0 o negativo, deshabilitar el campo de monto y el botón '+'
                    if (montoPendiente <= 0)
                    {
                        txtMontoPago.IsEnabled = false;
                        if (btnAgregarPago != null) btnAgregarPago.IsEnabled = false;
                    }
                    else
                    {
                        txtMontoPago.IsEnabled = true;
                        if (btnAgregarPago != null) btnAgregarPago.IsEnabled = true;
                    }
                }
                else
                {
                    idFormaPagoSeleccionada = 0;
                    MessageBox.Show($"Forma de pago '{nombreBoton}' no encontrada en la base de datos. Verifique la configuración de Formas de Pago.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAgregarPago_Click(object sender, RoutedEventArgs e)
        {
            if (idFormaPagoSeleccionada == 0)
            {
                MessageBox.Show("Selecciona una forma de pago primero (Efectivo, Transferencia, MP / Tarjetas).", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal monto;
            string mensajeError;
            if (!PagoService.ValidarMontoPago(txtMontoPago.Text.Trim(), out monto, out mensajeError))
            {
                MessageBox.Show(mensajeError, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Calcular el total de la venta y el monto pendiente antes de agregar este pago
            decimal totalVenta = 0;
            if (!decimal.TryParse(lblTotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out totalVenta))
            {
                MessageBox.Show("Total de la venta inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            decimal totalPagadoActual = pagosRealizados.Sum(p => p.Monto);
            decimal montoPendiente = totalVenta - totalPagadoActual;

            // --- LÓGICA: Prevenir o ajustar sobrepago ---
            // Si el monto a agregar es mayor que el pendiente, y aún hay pendiente positivo
            if (monto > montoPendiente && montoPendiente > 0)
            {
                MessageBox.Show($"El monto ingresado ({monto.ToString("C", CultureInfo.CurrentCulture)}) excede el monto pendiente ({montoPendiente.ToString("C", CultureInfo.CurrentCulture)}). Se ajustará el pago al monto pendiente.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                monto = montoPendiente; // Ajusta el monto al pendiente exacto
            }
            // Si ya no hay pendiente (o es negativo) y el usuario intenta agregar un monto positivo
            else if (montoPendiente <= 0)
            {
                MessageBox.Show("La venta ya ha sido cubierta. No se puede agregar más monto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Evita agregar el pago
            }
            // --- FIN LÓGICA ---


            // Obtener el nombre de la forma de pago para mostrar en la lista
            string nombreFormaPagoDisplay = formasPagoDisponibles.FirstOrDefault(fp => fp.IdFormaPago == idFormaPagoSeleccionada)?.NombreDisplay ?? "Desconocido";

            pagosRealizados.Add(new PagoTemporal
            {
                IdFormaPago = idFormaPagoSeleccionada,
                FormaPagoDisplay = nombreFormaPagoDisplay,
                Monto = monto
            });

            RefrescarUI();

            // Resetear campos de pago para el siguiente ingreso
            idFormaPagoSeleccionada = 0;
            txtMontoPago.Text = "0.00"; // Limpiar el campo de monto
            if (btnEfectivo != null) btnEfectivo.Background = Brushes.LightGray;
            if (btnTransferencia != null) btnTransferencia.Background = Brushes.LightGray;
            if (btnMpTarjetas != null) btnMpTarjetas.Background = Brushes.LightGray;
        }

        private void btnEliminarPago_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is PagoTemporal pagoToRemove)
            {
                pagosRealizados.Remove(pagoToRemove);
                RefrescarUI();
            }
        }

        #endregion

        #region Actualización de UI

        private void RefrescarUI()
        {
            lstProductos.Items.Refresh();
            lstCarrito.ItemsSource = null;
            lstCarrito.ItemsSource = carrito;
            lstCarrito.Items.Refresh();

            lstPagosRealizados.ItemsSource = null;
            lstPagosRealizados.ItemsSource = pagosRealizados;
            lstPagosRealizados.Items.Refresh();

            ActualizarTotales();
            lblCantidadItems.Text = carrito.Sum(item => item.Cantidad).ToString();
            ActualizarMontoPendiente();

            // Lógica de visibilidad del campo de monto a pagar
            decimal totalVentaTemp = 0;
            decimal.TryParse(lblTotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out totalVentaTemp);

            if (totalVentaTemp > 0 && idFormaPagoSeleccionada != 0) // Si hay productos y una forma de pago seleccionada
            {
                gridMontoPago.Visibility = Visibility.Visible;
                // Pre-llenar el campo de monto a pagar con el pendiente actual
                decimal totalPagado = pagosRealizados.Sum(p => p.Monto);
                decimal pendiente = totalVentaTemp - totalPagado;
                txtMontoPago.Text = pendiente.ToString("N2", CultureInfo.InvariantCulture);
            }
            else
            {
                gridMontoPago.Visibility = Visibility.Collapsed;
                txtMontoPago.Text = "0.00"; // Limpiar el texto cuando se oculta
            }
        }

        private void ActualizarTotales()
        {
            var (subtotal, montoDescuento, total) = DescuentoService.CalcularTotales(carrito, porcentajeDescuento);

            if (lblSubtotal != null) lblSubtotal.Text = subtotal.ToString("C", CultureInfo.CurrentCulture);
            if (lblDescuento != null) lblDescuento.Text = montoDescuento.ToString("C", CultureInfo.CurrentCulture);
            if (lblTotal != null) lblTotal.Text = total.ToString("C", CultureInfo.CurrentCulture);
        }

        private void ActualizarMontoPendiente()
        {
            decimal totalVenta = 0;
            if (decimal.TryParse(lblTotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out totalVenta))
            {
                decimal totalPagado = pagosRealizados.Sum(p => p.Monto);
                decimal pendiente = totalVenta - totalPagado;
                lblMontoPendiente.Text = $"Pendiente: {pendiente.ToString("C", CultureInfo.CurrentCulture)}";
                lblMontoPendiente.Foreground = (pendiente > 0) ? Brushes.Red : Brushes.Green;
            }
        }

        #endregion

        #region Descuentos

        private void chkDescuento15_Checked(object sender, RoutedEventArgs e)
        {
            porcentajeDescuento = 15;
            if (numDescuentoPersonalizado != null)
            {
                numDescuentoPersonalizado.Value = 0;
                numDescuentoPersonalizado.IsEnabled = false;
            }
            ActualizarTotales();
        }

        private void chkDescuento15_Unchecked(object sender, RoutedEventArgs e)
        {
            porcentajeDescuento = 0;
            if (numDescuentoPersonalizado != null)
            {
                numDescuentoPersonalizado.IsEnabled = true;
            }
            ActualizarTotales();
        }

        private void numDescuentoPersonalizado_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (numDescuentoPersonalizado != null && numDescuentoPersonalizado.Value.HasValue && numDescuentoPersonalizado.Value.Value > 0)
            {
                porcentajeDescuento = numDescuentoPersonalizado.Value.Value;
                if (chkDescuento15 != null) chkDescuento15.IsChecked = false;
            }
            else
            {
                porcentajeDescuento = 0;
            }
            ActualizarTotales();
        }

        #endregion

        #region Eventos de búsqueda y UI

        private void txtBuscarProducto_GotFocus(object sender, RoutedEventArgs e)
        {
            UiHelpers.SetPlaceholder(txtBuscarProducto, "Buscar producto...", true);
        }

        private void txtBuscarProducto_LostFocus(object sender, RoutedEventArgs e)
        {
            UiHelpers.SetPlaceholder(txtBuscarProducto, "Buscar producto...", false);
        }

        private void txtBuscarProducto_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (productosDisponibles == null || lstProductos == null)
                return;

            string textoBusqueda = txtBuscarProducto.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(textoBusqueda) || textoBusqueda == "buscar producto...")
            {
                lstProductos.ItemsSource = productosDisponibles;
            }
            else
            {
                var resultados = productosDisponibles
                    .Where(p =>
                        (p.Nombre != null && p.Nombre.ToLower().Contains(textoBusqueda)) ||
                        (p.Codigo != null && p.Codigo.ToLower().Contains(textoBusqueda)))
                    .ToList();
                lstProductos.ItemsSource = resultados;
            }
            lstProductos.Items.Refresh();
        }

        #endregion

        #region Venta rápida

        private void btnTipoRapido_Click(object sender, RoutedEventArgs e)
        {
            UiHelpers.ResetButtonBackground(btnVarios, btnFlor);

            if (sender is Button btn)
            {
                UiHelpers.SetButtonSelected(btn);
                tipoRapidoSeleccionado = btn.Content.ToString();
            }
        }

        private void btnAgregarRapido_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tipoRapidoSeleccionado))
            {
                MessageBox.Show("Selecciona un tipo: VARIOS o FLOR.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (txtImporteRapido == null || !decimal.TryParse(txtImporteRapido.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal importe) || importe <= 0)
            {
                MessageBox.Show("Ingresa un importe válido.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            VentaRapidaService.AgregarVentaRapida(carrito, tipoRapidoSeleccionado, importe);

            RefrescarUI();

            tipoRapidoSeleccionado = null;
            if (btnVarios != null) btnVarios.Background = Brushes.LightGray;
            if (btnFlor != null) btnFlor.Background = Brushes.LightGray;
            if (txtImporteRapido != null) txtImporteRapido.Text = "";
        }

        #endregion

        #region Finalización y helpers

        private void BtnFinalizarCompra_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal totalVenta = 0;
            if (!decimal.TryParse(lblTotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out totalVenta))
            {
                MessageBox.Show("Total de la venta inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal totalPagado = pagosRealizados.Sum(p => p.Monto);
            if (totalPagado < totalVenta)
            {
                MessageBox.Show($"El monto total pagado ({totalPagado.ToString("C", CultureInfo.CurrentCulture)}) es menor que el total de la venta ({totalVenta.ToString("C", CultureInfo.CurrentCulture)}).", "Error de Pago", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (totalPagado > totalVenta)
            {
                MessageBox.Show($"El monto total pagado ({totalPagado.ToString("C", CultureInfo.CurrentCulture)}) es mayor que el total de la venta ({totalVenta.ToString("C", CultureInfo.CurrentCulture)}). Se registrará el excedente.", "Advertencia de Pago", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            try
            {
                using var con = new SQLiteConnection(connectionString);
                con.Open();
                using var tran = con.BeginTransaction();

                // --- CORRECCIÓN AQUÍ: Comparación de CategoriaNombre y Codigo insensible a mayúsculas/minúsculas ---
                bool ventaContieneFlor = carrito.Any(item =>
                    (item.IdProducto != -1 && productosDisponibles.Any(p => p.IdProducto == item.IdProducto && p.CategoriaNombre != null && p.CategoriaNombre.Equals("Flor", StringComparison.OrdinalIgnoreCase))) ||
                    (item.IdProducto == -1 && item.Codigo != null && item.Codigo.Equals("FLOR", StringComparison.OrdinalIgnoreCase)));

                decimal montoTotalFlor = carrito.Where(item =>
                    (item.IdProducto != -1 && productosDisponibles.Any(p => p.IdProducto == item.IdProducto && p.CategoriaNombre != null && p.CategoriaNombre.Equals("Flor", StringComparison.OrdinalIgnoreCase))) ||
                    (item.IdProducto == -1 && item.Codigo != null && item.Codigo.Equals("FLOR", StringComparison.OrdinalIgnoreCase)))
                    .Sum(item => item.PrecioUnitario * item.Cantidad);
                // --- FIN CORRECCIÓN ---

                int idFormaPagoPrincipal = pagosRealizados.FirstOrDefault()?.IdFormaPago ?? 1;

                decimal subtotal = carrito.Sum(i => i.PrecioUnitario * i.Cantidad);
                decimal descuento = subtotal * (porcentajeDescuento / 100m);
                decimal totalFinalVenta = subtotal - descuento;

                int idCliente = 1;

                var cmdVenta = new SQLiteCommand(@"
                    INSERT INTO Ventas (
                        Fecha, IdCliente, IdPtoVenta, IdFormaPago, IdComprobante,
                        Subtotal, DescuentoPorcentaje, IVA, Total, TotalRecibido,
                        CAE, FechaVencimientoCAE, EsFlor, TotalFlor, RetencionIVA, RetencionGanancias)
                    VALUES (@F, @C, 1, @FP, @Comp, @Sub, @Desc, 0, @Tot, @Rec,
                            '', '', @EsFlor, @TotalFlor, @RetencionIVA, @RetencionGanancias);", con, tran);
                cmdVenta.Parameters.AddWithValue("@F", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmdVenta.Parameters.AddWithValue("@C", idCliente);
                cmdVenta.Parameters.AddWithValue("@FP", idFormaPagoPrincipal);
                cmdVenta.Parameters.AddWithValue("@Comp", 1);
                cmdVenta.Parameters.AddWithValue("@Sub", subtotal);
                cmdVenta.Parameters.AddWithValue("@Desc", (int)porcentajeDescuento);
                cmdVenta.Parameters.AddWithValue("@Tot", totalFinalVenta);
                cmdVenta.Parameters.AddWithValue("@Rec", totalPagado);
                cmdVenta.Parameters.AddWithValue("@EsFlor", ventaContieneFlor ? 1 : 0);
                cmdVenta.Parameters.AddWithValue("@TotalFlor", montoTotalFlor);
                cmdVenta.Parameters.AddWithValue("@RetencionIVA", 0.0m);
                cmdVenta.Parameters.AddWithValue("@RetencionGanancias", 0.0m);

                cmdVenta.ExecuteNonQuery();
                long idVenta = con.LastInsertRowId;

                foreach (var item in carrito)
                {
                    var cmdDetalle = new SQLiteCommand(@"
                        INSERT INTO DetalleVenta (IdVenta, IdProducto, Codigo, Descripcion, Cantidad, PrecioUnitario)
                        VALUES (@V, @P, @Cod, @Desc, @Cant, @PU);", con, tran);
                    cmdDetalle.Parameters.AddWithValue("@V", idVenta);
                    cmdDetalle.Parameters.AddWithValue("@P", item.IdProducto);
                    cmdDetalle.Parameters.AddWithValue("@Cod", item.Codigo);
                    cmdDetalle.Parameters.AddWithValue("@Desc", item.Nombre);
                    cmdDetalle.Parameters.AddWithValue("@Cant", item.Cantidad);
                    cmdDetalle.Parameters.AddWithValue("@PU", item.PrecioUnitario);
                    cmdDetalle.ExecuteNonQuery();

                    if (item.IdProducto != -1)
                    {
                        var cmdStock = new SQLiteCommand("UPDATE Producto SET Stock = Stock - @Q WHERE IdProducto = @ID;", con, tran);
                        cmdStock.Parameters.AddWithValue("@Q", item.Cantidad);
                        cmdStock.Parameters.AddWithValue("@ID", item.IdProducto);
                        cmdStock.ExecuteNonQuery();
                    }
                }

                foreach (var pago in pagosRealizados)
                {
                    var cmdPagoDetalle = new SQLiteCommand(@"
                        INSERT INTO DetallePagoVenta (IdVenta, IdFormaPago, MontoPagado)
                        VALUES (@IdVenta, @IdFormaPago, @MontoPagado);", con, tran);
                    cmdPagoDetalle.Parameters.AddWithValue("@IdVenta", idVenta);
                    cmdPagoDetalle.Parameters.AddWithValue("@IdFormaPago", pago.IdFormaPago);
                    cmdPagoDetalle.Parameters.AddWithValue("@MontoPagado", pago.Monto);
                    cmdPagoDetalle.ExecuteNonQuery();
                }

                tran.Commit();
                MessageBox.Show("Venta guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Preguntar si desea imprimir el ticket
                var imprimir = MessageBox.Show(
                    "¿Desea imprimir el ticket?",
                    "Imprimir ticket",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (imprimir == MessageBoxResult.Yes)
                {
                    ImpresoraTicket.ImprimirTicket(
                        carrito,
                        pagosRealizados,
                        subtotal,
                        descuento,
                        totalFinalVenta,
                        totalPagado,
                        DateTime.Now
                    );
                }

                ResetearPantalla(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la venta: " + ex.Message, "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetearPantalla(object sender, RoutedEventArgs e)
        {
            carrito.Clear();
            pagosRealizados.Clear();
            lstCarrito.ItemsSource = null;
            lstPagosRealizados.ItemsSource = null;
            CargarProductos();
            porcentajeDescuento = 0;
            if (chkDescuento15 != null) chkDescuento15.IsChecked = false;
            if (numDescuentoPersonalizado != null) numDescuentoPersonalizado.Value = 0;

            ActualizarTotales();

            if (txtBuscarProducto != null) txtBuscarProducto.Text = "Buscar producto...";
            if (txtImporteRapido != null) txtImporteRapido.Text = "";
            if (btnVarios != null) btnVarios.Background = Brushes.LightGray;
            if (btnFlor != null) btnFlor.Background = Brushes.LightGray;
            tipoRapidoSeleccionado = null;

            txtMontoPago.Text = "0.00";
            idFormaPagoSeleccionada = 0;
            if (btnEfectivo != null) btnEfectivo.Background = Brushes.LightGray;
            if (btnTransferencia != null) btnTransferencia.Background = Brushes.LightGray;
            if (btnMpTarjetas != null) btnMpTarjetas.Background = Brushes.LightGray;

            gridMontoPago.Visibility = Visibility.Collapsed;
        }

        private void btnFacturar_Click(object sender, RoutedEventArgs e)
        {
            FacturacionService.MostrarVentanaFacturacion(Window.GetWindow(this));
        }

        #endregion

        // --- BÚSQUEDA Y OTROS EVENTOS DE UI ---
        private void txtMontoPago_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Opcional: Puedes añadir lógica de validación o formato aquí si es necesario
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) { }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e) { }
    }
}

