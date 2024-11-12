using Npgsql;


namespace Coinqueror.TradeShifter.Data
{

    public class PostgresDbContext
    {
        private readonly NpgsqlConnection _connection;

        public PostgresDbContext(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
        }

        public NpgsqlConnection Connection => _connection;

        public void Open()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        public void Close()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }
}
