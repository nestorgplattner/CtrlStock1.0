using CapaPresentacionwpf.Model;
using CapaPresentacionWPF.Model;
using System.Data;
using System.Data.SQLite;

namespace CapaPresentacionWPF.Data
{
    public class ClienteData
    {
        private static string connectionString = $"Data Source=Data/stockdb.db;Version=3;";

        public static ClienteAFIP BuscarCliente(string identificador)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT Nombre, CUIT_CUIL, Domicilio, TipoDocumento
                    FROM Cliente
                    WHERE CUIT_CUIL = @id OR NºDocumento = @id
                    LIMIT 1";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", identificador);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ClienteAFIP
                            {
                                CUIT = reader["CUIT_CUIL"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Domicilio = reader["Domicilio"].ToString(),
                                CondicionIVA = reader["TipoDocumento"].ToString() // lo usamos como IVA x ahora
                            };
                        }
                    }
                }
            }
            return null;
        }

        public static void GuardarCliente(ClienteAFIP cliente)
        {
            using (var con = new SQLiteConnection(connectionString))
            {
                con.Open();

                // Revisar si ya existe
                string existeQuery = @"SELECT IdCliente FROM Cliente WHERE CUIT_CUIL = @cuit";
                using (var cmd = new SQLiteCommand(existeQuery, con))
                {
                    cmd.Parameters.AddWithValue("@cuit", cliente.CUIT);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // UPDATE
                        string update = @"
                            UPDATE Cliente
                            SET Nombre = @nombre,
                                Domicilio = @domicilio,
                                TipoDocumento = @tipo
                            WHERE CUIT_CUIL = @cuit";

                        using (var updateCmd = new SQLiteCommand(update, con))
                        {
                            updateCmd.Parameters.AddWithValue("@nombre", cliente.Nombre);
                            updateCmd.Parameters.AddWithValue("@domicilio", cliente.Domicilio);
                            updateCmd.Parameters.AddWithValue("@tipo", cliente.CondicionIVA);
                            updateCmd.Parameters.AddWithValue("@cuit", cliente.CUIT);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // INSERT
                        string insert = @"
                            INSERT INTO Cliente (Nombre, CUIT_CUIL, TipoDocumento, NºDocumento, Domicilio)
                            VALUES (@nombre, @cuit, @tipo, @doc, @domicilio)";

                        using (var insertCmd = new SQLiteCommand(insert, con))
                        {
                            insertCmd.Parameters.AddWithValue("@nombre", cliente.Nombre);
                            insertCmd.Parameters.AddWithValue("@cuit", cliente.CUIT);
                            insertCmd.Parameters.AddWithValue("@tipo", cliente.CondicionIVA);
                            insertCmd.Parameters.AddWithValue("@doc", cliente.CUIT); // por ahora usamos el mismo campo
                            insertCmd.Parameters.AddWithValue("@domicilio", cliente.Domicilio);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}