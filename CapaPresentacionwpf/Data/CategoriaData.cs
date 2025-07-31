using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Data
{
    public class CategoriaData
    {
        private string connectionString;

        public CategoriaData(string connString)
        {
            connectionString = connString;
        }

        /// <summary>
        /// Obtiene todas las categorías de la base de datos.
        /// </summary>
        public List<Categoria> GetAllCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT IdCategoria, Nombre, IdCategoriaPadre FROM Categoria ORDER BY Nombre;";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categorias.Add(new Categoria
                            {
                                IdCategoria = reader.GetInt32(reader.GetOrdinal("IdCategoria")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                IdCategoriaPadre = reader.IsDBNull(reader.GetOrdinal("IdCategoriaPadre")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("IdCategoriaPadre"))
                            });
                        }
                    }
                }
            }
            return categorias;
        }

        /// <summary>
        /// Obtiene solo las categorías principales (sin padre).
        /// </summary>
        public List<Categoria> ObtenerCategoriasPrincipales()
        {
            return GetAllCategorias().Where(c => c.IdCategoriaPadre == null).ToList();
        }

        /// <summary>
        /// Obtiene las subcategorías de una categoría principal.
        /// </summary>
        public List<Categoria> ObtenerSubcategorias(int idCategoriaPadre)
        {
            return GetAllCategorias().Where(c => c.IdCategoriaPadre == idCategoriaPadre).ToList();
        }

        /// <summary>
        /// Agrega una nueva categoría o subcategoría.
        /// </summary>
        public void AddCategoria(Categoria categoria)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Categoria (Nombre, IdCategoriaPadre) VALUES (@Nombre, @IdCategoriaPadre);";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", categoria.Nombre);
                    cmd.Parameters.AddWithValue("@IdCategoriaPadre", categoria.IdCategoriaPadre.HasValue ? (object)categoria.IdCategoriaPadre.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Actualiza una categoría existente.
        /// </summary>
        public void UpdateCategoria(Categoria categoria)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Categoria SET Nombre = @Nombre, IdCategoriaPadre = @IdCategoriaPadre WHERE IdCategoria = @IdCategoria;";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", categoria.IdCategoria);
                    cmd.Parameters.AddWithValue("@Nombre", categoria.Nombre);
                    cmd.Parameters.AddWithValue("@IdCategoriaPadre", categoria.IdCategoriaPadre.HasValue ? (object)categoria.IdCategoriaPadre.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Elimina una categoría por su Id.
        /// </summary>
        public void DeleteCategoria(int idCategoria)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Categoria WHERE IdCategoria = @IdCategoria;";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Elimina una categoría principal y sus subcategorías asociadas.
        /// </summary>
        public void EliminarCategoriaCompleta(int idCategoria)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Primero, elimina las subcategorías asociadas
                        var subcategorias = ObtenerSubcategorias(idCategoria);
                        foreach (var subcategoria in subcategorias)
                        {
                            DeleteCategoria(subcategoria.IdCategoria);
                        }

                        // Luego, elimina la categoría principal
                        DeleteCategoria(idCategoria);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Verifica si una categoría o subcategoría tiene productos asociados.
        /// </summary>
        public bool TieneProductosAsociados(int idCategoria, bool esCategoriaPrincipal)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query;
                if (esCategoriaPrincipal)
                {
                    query = @"SELECT COUNT(*) FROM Producto 
                              WHERE Categoria = @Id 
                              OR Subcategoria IN (SELECT IdCategoria FROM Categoria WHERE IdCategoriaPadre = @Id);";
                }
                else
                {
                    query = "SELECT COUNT(*) FROM Producto WHERE Categoria = @Id OR Subcategoria = @Id;";
                }

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idCategoria);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}
