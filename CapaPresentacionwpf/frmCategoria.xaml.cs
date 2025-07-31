using CapaPresentacionWPF.Data;
using CapaPresentacionWPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacionWPF
{
    public partial class frmCategoria : Window
    {
        private CategoriaData _categoriaData;
        private List<Categoria> categoriasPrincipales;
        private List<Categoria> subcategoriasSeleccionadas;
        private Categoria categoriaSeleccionada;

        private const string connectionString = @"Data Source=Data\stockdb.db;Version=3;";

        public frmCategoria()
        {
            InitializeComponent();
            _categoriaData = new CategoriaData(connectionString);
            CargarCategoriasPrincipales();
        }

        private void CargarCategoriasPrincipales()
        {
            categoriasPrincipales = _categoriaData.ObtenerCategoriasPrincipales();
            lstCategorias.ItemsSource = categoriasPrincipales;
            lstSubcategorias.ItemsSource = null;
            btnAgregarSubcategoria.IsEnabled = false;
        }

        private void CargarSubcategorias(int idCategoriaPadre)
        {
            subcategoriasSeleccionadas = _categoriaData.ObtenerSubcategorias(idCategoriaPadre);
            lstSubcategorias.ItemsSource = subcategoriasSeleccionadas;
            btnAgregarSubcategoria.IsEnabled = true;
        }

        private void LstCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            categoriaSeleccionada = lstCategorias.SelectedItem as Categoria;
            if (categoriaSeleccionada != null)
            {
                CargarSubcategorias(categoriaSeleccionada.IdCategoria);
            }
            else
            {
                lstSubcategorias.ItemsSource = null;
                btnAgregarSubcategoria.IsEnabled = false;
            }
        }

        private void BtnEliminarCategoria_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idCategoria)
            {
                var categoriaAEliminar = categoriasPrincipales.FirstOrDefault(c => c.IdCategoria == idCategoria);
                if (categoriaAEliminar == null) return;

                if (_categoriaData.TieneProductosAsociados(idCategoria, true))
                {
                    MessageBox.Show("No se puede eliminar esta categoría porque tiene productos asociados. Por favor, reasigne los productos primero.", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"¿Seguro que quieres eliminar la categoría '{categoriaAEliminar.Nombre}' y todas sus subcategorías?",
                                             "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _categoriaData.EliminarCategoriaCompleta(idCategoria);
                        CargarCategoriasPrincipales();
                        lstSubcategorias.ItemsSource = null;
                        btnAgregarSubcategoria.IsEnabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar la categoría: " + ex.Message, "Error");
                    }
                }
            }
        }

        private void BtnEliminarSubcategoria_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idSubcategoria)
            {
                var subcategoriaAEliminar = subcategoriasSeleccionadas.FirstOrDefault(s => s.IdCategoria == idSubcategoria);
                if (subcategoriaAEliminar == null) return;

                if (_categoriaData.TieneProductosAsociados(idSubcategoria, false))
                {
                    MessageBox.Show("No se puede eliminar esta subcategoría porque tiene productos asociados. Por favor, reasigne los productos primero.", "Error de Eliminación", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"¿Seguro que quieres eliminar la subcategoría '{subcategoriaAEliminar.Nombre}'?",
                                             "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _categoriaData.DeleteCategoria(idSubcategoria);
                        if (categoriaSeleccionada != null)
                            CargarSubcategorias(categoriaSeleccionada.IdCategoria);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar la subcategoría: " + ex.Message, "Error");
                    }
                }
            }
        }

        private void BtnAgregarCategoria_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new frmEditarCategoria();
            if (ventana.ShowDialog() == true)
            {
                // El resultado ya fue guardado en el diálogo, solo recargamos
                CargarCategoriasPrincipales();
            }
        }

        private void BtnAgregarSubcategoria_Click(object sender, RoutedEventArgs e)
        {
            if (categoriaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una categoría principal primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventana = new frmEditarCategoria(categoriaSeleccionada.IdCategoria);
            if (ventana.ShowDialog() == true)
            {
                // El resultado ya fue guardado en el diálogo, solo recargamos
                CargarSubcategorias(categoriaSeleccionada.IdCategoria);
            }
        }
    }
}
