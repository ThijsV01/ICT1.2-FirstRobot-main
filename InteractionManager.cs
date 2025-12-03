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
    private List<TimeSpan> interactionTimes = new()
    {
        new TimeSpan(9,0,0),
        new TimeSpan(15,0,0),
        new TimeSpan(18,0,0),
        new TimeSpan(21,0,0)
    };
    private List<TimeSpan> completedTimes = [];
    public bool IsInteractionTime()
    {
        TimeSpan now = DateTime.Now.TimeOfDay;

        //kijk of er een interactiemoment aan komt
        foreach (TimeSpan interactionTime in interactionTimes)
        {
            if (now.Hours == interactionTime.Hours && now.Minutes == interactionTime.Minutes && !completedTimes.Contains(interactionTime))
            {
                //interactiemoment komt
                completedTimes.Add(interactionTime);
                return true;
            }
        }
        //lijst met completed times leeghalen, zodat de volgende dag wel weer alles wordt uitgevoerd.
        if (now.TotalMinutes < 1)
        {
            completedTimes.Clear();
        }
        return false;
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