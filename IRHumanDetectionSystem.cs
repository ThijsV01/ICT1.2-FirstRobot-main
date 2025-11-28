using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
public class IRHumanDetectionSystem : IUpdatable
{
    //andere pin nummers en andere waardes in de variabelen zetten
    const int PIRMotionPinNumber = 26;
    const int MeasurementIntervalMilliseconds = 300;
    const int AlertTime = 300;
    private PIRMotion irHumanDetectionSensor;
    private PeriodTimer scanIntervalTimer;
    public int FoundHuman {get; private set;}

    public IRHumanDetectionSystem()
    {
        Console.WriteLine("IRHumanDetectionSystem constructor called");
        irHumanDetectionSensor = new PIRMotion(PIRMotionPinNumber,MeasurementIntervalMilliseconds,AlertTime);
        scanIntervalTimer = new PeriodTimer(MeasurementIntervalMilliseconds);
    }
    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            Robot.LEDs(0, 0, 255);
            FoundHuman = irHumanDetectionSensor.Watch();
            Console.WriteLine($"Human found: {FoundHuman}");
            Robot.LEDs(0, 0, 0);
        }
    }
}