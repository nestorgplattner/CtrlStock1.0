using System;

namespace CapaPresentacionWPF.Model
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; }
        // Esta es la propiedad que indica si es una subcategoría y a qué categoría padre pertenece
        // Basándome en tus errores, tu columna en la DB para esto se llama 'IdCategoriaPadre'.
        public int? IdCategoriaPadre { get; set; } // Id de la categoría padre (puede ser nulo para categorías principales)
    }
}