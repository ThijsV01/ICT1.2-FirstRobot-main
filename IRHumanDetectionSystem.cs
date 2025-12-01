using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
public class IRHumanDetectionSystem : IUpdatable
{
    const int PIRMotionPinNumber = 24;
    const int IntervalMilliseconds = 300;
    const int AlertTime = 1000;
    private PIRMotion irHumanDetectionSensor;
    private PeriodTimer scanIntervalTimer;
    public int FoundHuman {get; private set;}

    public IRHumanDetectionSystem()
    {
        Console.WriteLine("IRHumanDetectionSystem constructor called");
        irHumanDetectionSensor = new PIRMotion(PIRMotionPinNumber,IntervalMilliseconds,AlertTime);
        scanIntervalTimer = new PeriodTimer(IntervalMilliseconds);
    }
    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            FoundHuman = irHumanDetectionSensor.Watch();
            //Console.WriteLine($"Human found: {FoundHuman}");
        }
    }
}