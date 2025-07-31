using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CapaPresentacionWPF.Model;
using CapaPresentacionWPF.Data;

namespace CapaPresentacionWPF.UserControls
{
    public partial class ucRegistros : UserControl
    {
        private List<Producto> productosFiltrados;
        private List<Producto> productosOriginal;
        private readonly CategoriaData _categoriaData;
        private readonly ProductoData _productoData;
        private int paginaActual = 1;
        private int cantidadPorPagina = 20;
        private int totalPaginas = 1;

        // Define la cadena de conexión en un solo lugar
        private const string connectionString = @"Data Source=Data\stockdb.db;Version=3;";

        public ucRegistros()
        {
            InitializeComponent();
            // Pasa la cadena de conexión al constructor de las clases de datos
            _categoriaData = new CategoriaData(connectionString);
            _productoData = new ProductoData(connectionString);
            CargarProductos();
        }

        private void CargarProductos()
        {
            productosOriginal = _productoData.ObtenerProductos();
            productosFiltrados = productosOriginal;
            paginaActual = 1;
            totalPaginas = (int)Math.Ceiling((double)productosFiltrados.Count / cantidadPorPagina);
            MostrarPagina(paginaActual);
        }

        private void MostrarPagina(int numeroPagina)
        {
            var productosPagina = productosFiltrados
                .Skip((numeroPagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .ToList();

            dgProductos.ItemsSource = productosPagina;
            lblPagina.Content = $"Página {paginaActual} de {totalPaginas}";
            btnAnterior.IsEnabled = paginaActual > 1;
            btnSiguiente.IsEnabled = paginaActual < totalPaginas;
        }

        private void BtnAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                MostrarPagina(paginaActual);
            }
        }

        private void BtnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                MostrarPagina(paginaActual);
            }
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(filtro))
            {
                productosFiltrados = productosOriginal;
            }
            else
            {
                productosFiltrados = productosOriginal
                    .Where(p =>
                        (!string.IsNullOrEmpty(p.Nombre) && p.Nombre.ToLower().Contains(filtro)) ||
                        (!string.IsNullOrEmpty(p.Codigo) && p.Codigo.ToLower().Contains(filtro)))
                    .ToList();
            }

            totalPaginas = (int)Math.Ceiling((double)productosFiltrados.Count / cantidadPorPagina);
            paginaActual = 1;
            MostrarPagina(paginaActual);
        }

        private void BtnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new frmNuevoProducto();
            bool? resultado = ventana.ShowDialog();
            if (resultado == true)
                CargarProductos();
        }

        private void BtnEditarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var seleccionado = button?.DataContext as Producto;

            if (seleccionado != null)
            {
                var ventanaEdicion = new frmNuevoProducto(seleccionado);
                if (ventanaEdicion.ShowDialog() == true)
                {
                    CargarProductos();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para editar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnEliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var seleccionado = button?.DataContext as Producto;

            if (seleccionado != null)
            {
                var result = MessageBox.Show($"¿Seguro que querés eliminar '{seleccionado.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _productoData.EliminarProducto(seleccionado.IdProducto);
                        MessageBox.Show("Producto eliminado correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarProductos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar el producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnEditarCategorias_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new frmCategoria();
            ventana.ShowDialog();
        }
    }
}
