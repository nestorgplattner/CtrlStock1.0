using CapaPresentacionWPF.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace CapaPresentacionWPF.Data
{
    public static class RegistroData
    {
        private static string connectionString = @"Data Source=Data\stockdb.db;Version=3;";

        public static List<RegistroVenta> ObtenerUltimasVentas()
        {
            List<RegistroVenta> lista = new List<RegistroVenta>();
            using (var con = new SQLiteConnection(connectionString))
            {
                con.Open();
                string query = @"SELECT Fecha, FormaPago, Total FROM Ventas ORDER BY Fecha DESC LIMIT 100";
                using (var cmd = new SQLiteCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new RegistroVenta
                        {
                            Fecha = reader["Fecha"].ToString(),
                            FormaPago = reader["FormaPago"].ToString(),
                            Total = Convert.ToDecimal(reader["Total"])
                        });
                    }
                }
            }
            return lista;
        }
    }
}