using System;
using System.ComponentModel; // Si usas INotifyPropertyChanged
using System.Runtime.CompilerServices; // Si usas INotifyPropertyChanged

namespace CapaPresentacionWPF.Model
{
    public class Producto // Si usas INotifyPropertyChanged, añade : INotifyPropertyChanged
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int Stock { get; set; }
        public decimal Precio { get; set; } // Precio de Venta (normal)
        public int Estado { get; set; }

        public decimal PrecioCompra { get; set; }
        public decimal PorcentajeGanancia { get; set; }
        public decimal PrecioFinal { get; set; }
        public int Enoferta { get; set; } // 0 o 1
        public string OfertaHasta { get; set; } // Formato YYYY-MM-DD

        // Estas son las propiedades que deben coincidir con las columnas FK en la tabla Producto
        // Si tus columnas en la DB se llaman 'Categoria' y 'Subcategoria', estas FK deben llamarse así en el modelo.
        public int Categoria { get; set; } // FK a Categoria.IdCategoria para la categoría principal
        public int? Subcategoria { get; set; } // FK a Categoria.IdCategoria para la subcategoría (puede ser nulo)

        // Propiedades para mostrar en la UI (los nombres de las categorías/subcategorías)
        public string CategoriaNombre { get; set; }
        public string SubcategoriaNombre { get; set; }

        /* Si quieres implementar INotifyPropertyChanged (recomendado para WPF Bindings):
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        */
    }
}