using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace CapaPresentacionWPF.Data
{
    public static class SQLiteConnectionManager
    {
        private static SQLiteConnection? _connection = null;
        private static readonly object _lock = new object();

        // La cadena de conexión que usas actualmente, agrega BusyTimeout para evitar locks temporales
        private static readonly string _connectionString = @"Data Source=Data\Stockdb.db;Version=3;BusyTimeout=5000;";

        // Obtenemos la conexión compartida
        public static SQLiteConnection GetConnection()
        {
            lock (_lock)
            {
                if (_connection == null)
                {
                    _connection = new SQLiteConnection(_connectionString);
                    _connection.Open();
                }
                else if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        // Este objeto es para usar lock afuera (en los métodos)
        public static object LockObject => _lock;

        // Opcional: cierra la conexión al cerrar la app
        public static void CloseConnection()
        {
            lock (_lock)
            {
                if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }
    }
}
