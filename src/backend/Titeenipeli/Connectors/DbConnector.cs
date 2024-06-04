using System.Data;
using Npgsql;

namespace Titeenipeli.Connectors;

public class DbConnector
{
    private readonly NpgsqlConnection _connection;
    private readonly string _connectionString;

    public DbConnector(
        string connectionString = "Server=127.0.0.1;Port=5432;Userid=titeenipeli;Timeout=15;Database=titeenipeli")
    {
        _connectionString = connectionString;
        _connection = new NpgsqlConnection(_connectionString);
    }

    public DataSet ExecuteCommand(string command)
    {
        DataSet dataSet = new DataSet("ResultDataSet");

        using NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command, _connection);

        adapter.Fill(dataSet);
        return dataSet;
    }
}