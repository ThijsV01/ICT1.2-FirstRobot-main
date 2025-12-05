using HiveMQtt.MQTT5.Types;
using Microsoft.Data.SqlClient;
using SimpleMqtt;
public class BatteryService : IBatteryService
{
     public void InsertBattery(int batteryValue)
     {
    //     using SqlConnection connection = new SqlConnection(DatabaseConnection.connString);
    //     connection.Open();
    //     Console.WriteLine("servernaam:" +connection.Database);
    //     using SqlCommand command = connection.CreateCommand();
    //     command.CommandText = "Insert into BatterijStatus (BatterijId, RobotId, Tijdstip, Percentage) values( @BatterijId, @RobotId , @Tijdstip, @Percentage)";
    //     command.Parameters.AddWithValue("@Percentage", batteryValue);
    //     command.ExecuteNonQuery();
    //     connection.Close();
     }


}