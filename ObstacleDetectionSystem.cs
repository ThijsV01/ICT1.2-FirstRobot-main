using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
public class ObstacleDetectionSystem : IUpdatable
{
    const int UltrasonicPinNumber = 26;
    const int MeasurementIntervalMilliseconds = 300;

    private Ultrasonic distanceSensor;
    private PeriodTimer scanIntervalTimer;
    public int ObstacleDistance {get; private set;}

    public ObstacleDetectionSystem()
    {
        Console.WriteLine("ObstacleDetectionSystem constructor called");
        distanceSensor = new Ultrasonic(UltrasonicPinNumber);
        scanIntervalTimer = new PeriodTimer(MeasurementIntervalMilliseconds);
    }
    public void Update()
    {
        if (scanIntervalTimer.Check())
        {
            Robot.LEDs(0, 0, 255);
            ObstacleDistance = distanceSensor.GetUltrasoneDistance();
            Console.WriteLine($"New distance: {ObstacleDistance} cm");
            Robot.LEDs(0, 0, 0);
        }
    }
}