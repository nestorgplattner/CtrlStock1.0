using CapaPresentacionwpf.Model;
using CapaPresentacionWPF.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CapaPresentacionWPF.Data
{
    public class ProductoData
    {
        private string connectionString;

        public ProductoData(string connString)
        {
            connectionString = connString;
        }

        public List<Producto> ObtenerProductos()
        {
            List<Producto> productos = new List<Producto>();
            string query = @"
                SELECT 
                    p.IdProducto, p.Codigo, p.Nombre, p.Precio, p.PrecioCompra, p.PorcentajeGanancia, p.PrecioFinal, p.Stock, p.Enoferta, p.OfertaHasta,
                    c.Nombre AS CategoriaNombre,
                    sc.Nombre AS SubcategoriaNombre
                FROM Producto AS p
                LEFT JOIN Categoria AS c ON p.Categoria = c.IdCategoria
                LEFT JOIN Categoria AS sc ON p.Subcategoria = sc.IdCategoria;
            ";

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productos.Add(new Producto
                            {
                                IdProducto = reader.GetInt32(reader.GetOrdinal("IdProducto")),
                                Codigo = reader.GetString(reader.GetOrdinal("Codigo")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Precio = reader.GetDecimal(reader.GetOrdinal("Precio")),
                                PrecioCompra = reader.GetDecimal(reader.GetOrdinal("PrecioCompra")),
                                PorcentajeGanancia = reader.GetDecimal(reader.GetOrdinal("PorcentajeGanancia")),
                                PrecioFinal = reader.GetDecimal(reader.GetOrdinal("PrecioFinal")),
                                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                                Enoferta = reader.GetInt32(reader.GetOrdinal("Enoferta")),
                               // Producto.FechaVencimiento = reader["FechaVencimiento"] as DateTime?;
                                OfertaHasta = reader.IsDBNull(reader.GetOrdinal("OfertaHasta")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("OfertaHasta")),
                                CategoriaNombre = reader.IsDBNull(reader.GetOrdinal("CategoriaNombre")) ? "Sin Categoría" : reader.GetString(reader.GetOrdinal("CategoriaNombre")),
                                SubcategoriaNombre = reader.IsDBNull(reader.GetOrdinal("SubcategoriaNombre")) ? "Sin Subcategoría" : reader.GetString(reader.GetOrdinal("SubcategoriaNombre"))
                            });
                        }
                    }
                }
            }
            return productos;
        }

        public void EliminarProducto(int idProducto)
        {
            string query = "DELETE FROM Producto WHERE IdProducto = @IdProducto;";
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
