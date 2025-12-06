using System.Data;
using Microsoft.Data.SqlClient;
public class SqlCommandoRepository : ISqlCommandoRepository
{
    private string _connectionString;

    public SqlCommandoRepository (string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> GetCommands()
    {
        // using SqlConnection connection = new SqlConnection(_connectionString);
        // await connection.OpenAsync();

        // using SqlCommand command = connection.CreateCommand();
        // command.CommandText = "SELECT TOP 1 Percentage FROM BatterijStatus ORDER BY Tijdstip DESC";

        // using SqlDataReader reader = await command.ExecuteReaderAsync();

        // if (await reader.ReadAsync())
        // {
        //     return reader.GetInt32(0);
        // }

        return 0;
    }
    public async void InsertCommand()
    {
        // try
        // {
        //     using SqlConnection connection = new SqlConnection(_connectionString);
        //     await connection.OpenAsync();

        //     using SqlCommand command = connection.CreateCommand();
        //     command.CommandText = "INSERT INTO BatterijStatus (RobotId, Tijdstip, Percentage) VALUES (@RobotId, @Tijdstip, @Percentage)";
        //     command.Parameters.Add("@RobotId", SqlDbType.Int).Value = robotId;
        //     command.Parameters.Add("@Tijdstip", SqlDbType.DateTime).Value = DateTime.Now;
        //     command.Parameters.Add("@Percentage", SqlDbType.Int).Value = batteryValue;

        //     await command.ExecuteNonQueryAsync();
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine("Error inserting battery level: " + ex.Message);
        // }
    }
}