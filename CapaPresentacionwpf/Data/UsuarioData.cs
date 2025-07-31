using System;
using System.Data.SQLite;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Data
{
    public class UsuarioData
    {
        private readonly string connectionString = @"Data Source=Data\stockdb.db;Version=3;";

        public Usuario Login(string usuario, string clave)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Usuarios WHERE Usuario = @usuario AND Clave = @clave";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@clave", clave);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                UsuarioNombre = reader["Usuario"].ToString(),
                                Clave = reader["Clave"].ToString(),
                                Rol = reader["Rol"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}