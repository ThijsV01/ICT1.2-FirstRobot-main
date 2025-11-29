using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
public class InteractionManager
{
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
            if (now >= interactionTime && !completedTimes.Contains(interactionTime))
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
        //als parameters de tijd,datum etc meegeven om in database te zetten
        Robot.PlayNotes("L16EGC6G6");
    }

}