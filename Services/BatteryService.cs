using HiveMQtt.MQTT5.Types;
using Microsoft.Data.SqlClient;
using SimpleMqtt;
public class BatteryService : IBatteryService
{
    public void InsertBattery()
    {
        using SqlConnection connection = new SqlConnection(DatabaseConnection.connString);
        connection.Open();
        Console.WriteLine("servernaam:" +connection.Database);
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "Insert into BatterijStatus (BatterijId, RobotId, Tijdstip, Percentage) values( @BatterijId, @RobotId , @Tijdstip, @Percentage)";
        command.Parameters.AddWithValue("@BatterijId", Bid);
        command.Parameters.AddWithValue("@BatterijId", Rid);
        command.Parameters.AddWithValue("@BatterijId", Tijdstip);
        command.Parameters.AddWithValue("@BatterijId", Percentage);
        command.ExecuteNonQuery();
        connection.Close();
    }


}