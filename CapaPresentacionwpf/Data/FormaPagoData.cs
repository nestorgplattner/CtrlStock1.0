using System;
using System.Collections.Generic;
using System.Data.SQLite;
using CapaPresentacionWPF.Model;

namespace CapaPresentacionWPF.Data
{
    // Esta clase se encarga de las operaciones de la base de datos para las formas de pago.
    public class FormaPagoData
    {
        // Cadena de conexión a la base de datos.
        private readonly string _connectionString;

        public FormaPagoData(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Función para obtener todas las formas de pago de la tabla FormaPago.
        public List<FormaPagoSimple> ObtenerFormasPago()
        {
            var formasPago = new List<FormaPagoSimple>();

            // Consulta SQL corregida para seleccionar las columnas que existen en la tabla.
            string query = "SELECT IdFormaPago, Plataforma, MedioPago FROM FormaPago ORDER BY Plataforma, MedioPago;";

            try
            {
                // Establece la conexión a la base de datos SQLite.
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        // Lee los datos de la base de datos.
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Crea un nuevo objeto FormaPagoSimple para cada registro.
                                var formaPago = new FormaPagoSimple
                                {
                                    IdFormaPago = reader.GetInt32(reader.GetOrdinal("IdFormaPago")),
                                    Plataforma = reader.IsDBNull(reader.GetOrdinal("Plataforma")) ? "" : reader.GetString(reader.GetOrdinal("Plataforma")),
                                    MedioPago = reader.IsDBNull(reader.GetOrdinal("MedioPago")) ? "" : reader.GetString(reader.GetOrdinal("MedioPago"))
                                };
                                formasPago.Add(formaPago);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener formas de pago: {ex.Message}");
                // En un entorno de producción, es mejor manejar el error de otra forma,
                // como loguear la excepción o mostrar un mensaje de error en la UI.
            }

            return formasPago;
        }

        
    }
}
