using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using SimpleMqtt;
public class InteractionManager
{
    private readonly SimpleMqttClient _mqttClient;
    public InteractionManager(SimpleMqttClient client)
    {
        _mqttClient=client;
    }
    public void StartActivity()
    {
        DateTime startMoment=DateTime.Now;
        _mqttClient.PublishMessage(startMoment.ToString("dd-MM-yyyy"),"robot/2242722/interaction");
        Robot.PlayNotes("L16EGC6G6");
        Robot.Wait(500);
        Console.WriteLine(startMoment);
        Robot.Wait(500);
    }

}