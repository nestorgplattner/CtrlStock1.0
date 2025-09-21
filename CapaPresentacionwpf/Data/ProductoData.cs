using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Data
{
    public class ProductoData
    {
        private readonly string connString;

        // Constructor para inicializar la cadena de conexi�n
        public ProductoData(string connString)
        {
            this.connString = connString;

        }

        /// <summary>
        /// Obtiene una lista de todos los productos disponibles en la base de datos.
        /// </summary>
        /// <returns>Una lista de objetos Producto.</returns>
        public List<Producto> ObtenerProductos()
        {
            var productos = new List<Producto>();

            // Sentencia SQL para seleccionar todos los productos con sus categor�as.
            // Hacemos un JOIN para obtener el nombre de la categor�a en lugar del ID.
            string sql = @"
                SELECT 
                    p.IdProducto,
                    p.Codigo,
                    p.Nombre,
                    p.Stock,
                    p.PrecioFinal,
                    c.Nombre AS CategoriaNombre -- Se renombra para evitar conflictos
                FROM Producto AS p
                LEFT JOIN Categoria AS c ON p.Categoria = c.IdCategoria
                WHERE p.Estado IS NULL OR p.Estado = 1; -- Filtramos solo los productos activos
            ";

            try
            {
                // Usamos "using" para asegurar que la conexi�n se cierre correctamente
                using (var con = new SQLiteConnection(connString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                productos.Add(new Producto
                                {
                                    IdProducto = reader.GetInt32(reader.GetOrdinal("IdProducto")),
                                    Codigo = reader["Codigo"] as string,
                                    Nombre = reader["Nombre"] as string,
                                    Stock = reader.IsDBNull(reader.GetOrdinal("Stock")) ? 0 : reader.GetInt32(reader.GetOrdinal("Stock")),
                                    PrecioFinal = reader.IsDBNull(reader.GetOrdinal("PrecioFinal")) ? 0.0m : reader.GetDecimal(reader.GetOrdinal("PrecioFinal")),
                                    // Se agrega la categor�a a la clase Producto para su uso en la UI
                                    CategoriaNombre = reader["CategoriaNombre"] as string
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores: imprime la excepci�n para depuraci�n.
                Console.WriteLine($"Error al obtener productos: {ex.Message}");
                // En una aplicaci�n real, se deber�a registrar el error y/o notificar al usuario.
            }

            return productos;
        }

        // M�todos pendientes
        public void AddProducto(Producto prod)
        {
            // TODO: INSERT INTO Productos (...)
        }

        public void UpdateProducto(Producto prod)
        {
            // TODO: UPDATE Productos SET ... WHERE IdProducto = ...
        }

        public void EliminarProducto(int idProducto)
        {
            // ya lo ten�s, o bien asegurate de su existencia
        }
    }
}


