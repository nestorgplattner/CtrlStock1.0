using CapaPresentacionWPF.Data;
using CapaPresentacionWPF.Model;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacionWPF
{
    public partial class frmEditarCategoria : Window
    {
        private CategoriaData _categoriaData;
        private Categoria _categoriaOriginal;

        public Categoria CategoriaResult { get; private set; }

        private const string connectionString = @"Data Source=Data\stockdb.db;Version=3;";

        // Constructor para agregar una nueva categoría (principal o sub)
        public frmEditarCategoria(int? idCategoriaPadre = null)
        {
            InitializeComponent();
            _categoriaData = new CategoriaData(connectionString);
            _categoriaOriginal = new Categoria { IdCategoriaPadre = idCategoriaPadre };
            this.DataContext = _categoriaOriginal;
        }

        // Constructor para editar una categoría existente
        public frmEditarCategoria(Categoria categoria)
        {
            InitializeComponent();
            _categoriaData = new CategoriaData(connectionString);
            _categoriaOriginal = categoria;
            this.DataContext = _categoriaOriginal;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Advertencia: Asegúrate de que tu XAML (frmEditarCategoria.xaml)
            // contenga un TextBox con el nombre 'txtNombreCategoria'.
            if (txtNombreCategoria == null)
            {
                MessageBox.Show("El control 'txtNombreCategoria' no se encontró.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string nombre = txtNombreCategoria.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El nombre de la categoría no puede estar vacío.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _categoriaOriginal.Nombre = nombre;
                if (_categoriaOriginal.IdCategoria == 0)
                {
                    _categoriaData.AddCategoria(_categoriaOriginal);
                }
                else
                {
                    _categoriaData.UpdateCategoria(_categoriaOriginal);
                }

                CategoriaResult = _categoriaOriginal;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la categoría: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
