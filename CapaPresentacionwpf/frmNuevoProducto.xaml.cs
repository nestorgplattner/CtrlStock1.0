using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using CapaPresentacionWPF.Model; // Asegúrate que este namespace es correcto

using CapaPresentacionWPF.Data; // Asegúrate que este namespace es correcto

namespace CapaPresentacionWPF // Asegúrate que este namespace es correcto
{
    public partial class frmNuevoProducto : Window
    {
        private string connectionString = @"Data Source=Data\stockdb.db;Version=3;";
        private Producto productoActual; // Null para nuevo, objeto para editar
        private ProductoData productoData;
        private CategoriaData categoriaData;

        public ObservableCollection<Categoria> CategoriasPrincipales { get; set; }
        public ObservableCollection<Categoria> SubcategoriasDisponibles { get; set; }
        private List<Categoria> todasLasCategorias; // Para filtrar subcategorías


        // Constructor para AÑADIR nuevo producto
        public frmNuevoProducto()
        {
            InitializeComponent();
            productoData = new ProductoData(connectionString);
            categoriaData = new CategoriaData(connectionString);

            productoActual = new Producto { Estado = 1, Enoferta = 0 }; // Valores predeterminados para nuevo
            this.Title = "Nuevo Producto";

            CargarCategoriasIniciales();
            this.DataContext = productoActual; // Bindea el producto a la UI

            // Configurar eventos para cálculo automático y visibilidad de oferta
            txtPrecioCompra.LostFocus += CalcularPrecioFinal;
            txtPorcentajeGanancia.LostFocus += CalcularPrecioFinal;
            //chkEnOferta.Checked += ChkEnOferta_Changed;
            //chkEnOferta.Unchecked += ChkEnOferta_Changed;

            // Asegurarse de que el DatePicker no esté habilitado por defecto si no hay oferta
            dpOfertaHasta.IsEnabled = false;
        }

        // Constructor para EDITAR producto existente
        public frmNuevoProducto(Producto producto) // Este constructor recibe un producto para editar
        {
            InitializeComponent();
            productoData = new ProductoData(connectionString);
            categoriaData = new CategoriaData(connectionString);

            productoActual = producto;
            this.Title = "Editar Producto";

            CargarCategoriasIniciales();
            this.DataContext = productoActual; // Bindea el producto a la UI

            // Seleccionar categorías y subcategorías existentes
            if (productoActual.Categoria > 0) // Usamos la propiedad 'Categoria'
            {
                cmbCategoria.SelectedValue = productoActual.Categoria;
                // Esto cargará las subcategorías y luego podremos seleccionar la correcta
                CargarSubcategoriasPorCategoria(productoActual.Categoria); // Usamos la propiedad 'Categoria'
                // Asegúrate de que SubcategoriasDisponibles ya está poblada antes de intentar seleccionar
                if (productoActual.Subcategoria.HasValue) // Usamos la propiedad 'Subcategoria'
                {
                    cmbSubcategoria.SelectedValue = productoActual.Subcategoria.Value;
                }
            }

            // Establecer fecha de oferta si existe
            if (!string.IsNullOrEmpty(productoActual.OfertaHasta))
            {
                if (DateTime.TryParse(productoActual.OfertaHasta, out DateTime ofertaDate))
                {
                    dpOfertaHasta.SelectedDate = ofertaDate;
                }
            }

            // Configurar eventos para cálculo automático y visibilidad de oferta
            txtPrecioCompra.LostFocus += CalcularPrecioFinal;
            txtPorcentajeGanancia.LostFocus += CalcularPrecioFinal;
            //chkEnOferta.Checked += ChkEnOferta_Changed;
            //chkEnOferta.Unchecked += ChkEnOferta_Changed;

            // Establecer estado inicial del DatePicker de oferta
            //dpOfertaHasta.IsEnabled = productoActual.Enoferta == 1;
        }

        private void CargarCategoriasIniciales()
        {
            todasLasCategorias = categoriaData.GetAllCategorias();
            CategoriasPrincipales = new ObservableCollection<Categoria>(todasLasCategorias.Where(c => c.IdCategoriaPadre == null)); // Usamos IdCategoriaPadre
            SubcategoriasDisponibles = new ObservableCollection<Categoria>(); // Inicialmente vacío

            // Necesario para el binding de los ComboBoxes
            cmbCategoria.ItemsSource = CategoriasPrincipales;
            cmbCategoria.DisplayMemberPath = "Nombre";
            cmbCategoria.SelectedValuePath = "IdCategoria";

            cmbSubcategoria.ItemsSource = SubcategoriasDisponibles;
            cmbSubcategoria.DisplayMemberPath = "Nombre";
            cmbSubcategoria.SelectedValuePath = "IdCategoria";
        }

        private void cmbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategoria.SelectedValue != null)
            {
                int idCategoriaSeleccionada = (int)cmbCategoria.SelectedValue;
                CargarSubcategoriasPorCategoria(idCategoriaSeleccionada);
            }
            else
            {
                SubcategoriasDisponibles.Clear(); // Limpiar si no hay categoría seleccionada
            }
            // Después de cambiar la categoría principal, si hay una subcategoría seleccionada previamente, deseleccionarla
            cmbSubcategoria.SelectedValue = null;
        }

        private void CargarSubcategoriasPorCategoria(int idCategoriaPadre)
        {
            SubcategoriasDisponibles.Clear();
            var subcategoriasFiltradas = todasLasCategorias.Where(c => c.IdCategoriaPadre == idCategoriaPadre); // Usamos IdCategoriaPadre
            foreach (var subcat in subcategoriasFiltradas)
            {
                SubcategoriasDisponibles.Add(subcat);
            }
        }

        private void CalcularPrecioFinal(object sender, RoutedEventArgs e)
        {
            decimal precioCompra = 0;
            decimal porcentajeGanancia = 0;

            if (decimal.TryParse(txtPrecioCompra.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out precioCompra) &&
                decimal.TryParse(txtPorcentajeGanancia.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out porcentajeGanancia))
            {
                productoActual.PrecioCompra = precioCompra;
                productoActual.PorcentajeGanancia = porcentajeGanancia;
                productoActual.PrecioFinal = precioCompra * (1 + (porcentajeGanancia / 100));
                lblPrecioFinalCalculado.Text = productoActual.PrecioFinal.ToString("C", CultureInfo.CurrentCulture); // Muestra el precio final calculado
            }
            else
            {
                lblPrecioFinalCalculado.Text = "Cálculo inválido"; // Mensaje de error si la entrada no es válida
            }
        }

        private void ChkEnOferta_Changed(object sender, RoutedEventArgs e)
        {
            //dpOfertaHasta.IsEnabled = chkEnOferta.IsChecked == true;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // --- Validación de campos ---
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtStock.Text) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text) || // Precio es el precio de venta normal
                string.IsNullOrWhiteSpace(txtPrecioCompra.Text) ||
                string.IsNullOrWhiteSpace(txtPorcentajeGanancia.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos obligatorios (Código, Nombre, Stock, Precio, Precio Compra, % Ganancia).", "Campos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("El Stock debe ser un número entero no negativo.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal precio) || precio < 0)
            {
                MessageBox.Show("El Precio debe ser un número válido no negativo.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecioCompra.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal precioCompra) || precioCompra < 0)
            {
                MessageBox.Show("El Precio Compra debe ser un número válido no negativo.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPorcentajeGanancia.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal porcentajeGanancia) || porcentajeGanancia < 0)
            {
                MessageBox.Show("El % Ganancia debe ser un número válido no negativo.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbCategoria.SelectedValue == null)
            {
                MessageBox.Show("Por favor, seleccione una Categoría.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Solo valida la subcategoría si hay subcategorías disponibles para la categoría seleccionada
            if (SubcategoriasDisponibles.Any() && cmbSubcategoria.SelectedValue == null)
            {
                MessageBox.Show("Por favor, seleccione una Subcategoría para la categoría elegida.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //if (chkEnOferta.IsChecked == true && !dpOfertaHasta.SelectedDate.HasValue)
            //{
               // MessageBox.Show("Debe seleccionar una fecha 'Oferta Hasta' si el producto está en oferta.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                //return;
            //}

            // Actualizar el objeto productoActual con los valores de la UI
            productoActual.Codigo = txtCodigo.Text;
            productoActual.Nombre = txtNombre.Text;
            productoActual.Stock = stock;
            productoActual.Precio = precio; // Precio de venta normal

            productoActual.Categoria = (int)cmbCategoria.SelectedValue; // Usamos la propiedad 'Categoria'
            productoActual.Subcategoria = cmbSubcategoria.SelectedValue != null ? (int)cmbSubcategoria.SelectedValue : (int?)null; // Usamos la propiedad 'Subcategoria'

            productoActual.PrecioCompra = precioCompra;
            productoActual.PorcentajeGanancia = porcentajeGanancia;

            // Asegúrate de que lblPrecioFinalCalculado.Text contiene un valor parseable
            if (decimal.TryParse(lblPrecioFinalCalculado.Text.Replace(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, "").Trim(), NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal precioFinalCalculado))
            {
                productoActual.PrecioFinal = precioFinalCalculado;
            }
            else
            {
                // Fallback o mensaje de error si el cálculo no fue válido
                productoActual.PrecioFinal = precio; // O algún valor por defecto
                MessageBox.Show("El Precio Final calculado no es válido. Se usará el Precio de Venta.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            //productoActual.Enoferta = chkEnOferta.IsChecked == true ? 1 : 0;
            //productoActual.OfertaHasta = chkEnOferta.IsChecked == true && dpOfertaHasta.SelectedDate.HasValue ? dpOfertaHasta.SelectedDate.Value.ToString("yyyy-MM-dd") : "";
            //productoActual.Estado = 1; // Asumiendo que 1 es activo al guardar

            try
            {
                if (productoActual.IdProducto == 0) // Nuevo Producto
                {
                    productoData.AddProducto(productoActual);
                }
                else // Editar Producto
                {
                    productoData.UpdateProducto(productoActual);
                }
                this.DialogResult = true; // Indica éxito
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el producto: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Indica cancelación
            this.Close();
        }
    }
}