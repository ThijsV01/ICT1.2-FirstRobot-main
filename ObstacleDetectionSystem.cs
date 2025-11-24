using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

/// <summary>
/// This class uses ultrasonic sensors to measure distance to obstacles
/// in front of the robot and makes this distance avaiable as a property ObstacleDistance
/// The intervals with which the ultrasonic sensors are triggered is determined
/// by an internal constant
/// Implements interface IUpdatable, so method Update() must be called frequently
/// </summary>
public class ObstacleDetectionSystem : IUpdatable
{
    // Define which pin is used for the ultrasonic distance sensor
    // Note: this could also be passed to us in the constructor call
    //       and that would be cleaner, we wouldn't have to change
    //       our code here in case of a pin change
    const int UltrasonicPinNumber = 26;
    
    // Define how many times per second we perform distance measurements
    // Note: each measurement takes a little bit of time (typically up to 50 milliseconds)
    //       in which the robot software is not performing other updates
    //       so choosing this interval is a compromise
    const int MeasurementIntervalMilliseconds = 300;

    private Ultrasonic distanceSensor;
    private PeriodTimer scanIntervalTimer; // Used to determine if a new measurement is due

    /// <summary>
    /// Most recently measured distance in cm to an obstacle in front of the robot
    /// </summary>
    public int ObstacleDistance {get; private set;} // Can only be read, not written by other classes

    public ObstacleDetectionSystem()
    {
        Console.WriteLine("ObstacleDetectionSystem constructor called");
        distanceSensor = new Ultrasonic(UltrasonicPinNumber);
        scanIntervalTimer = new PeriodTimer(MeasurementIntervalMilliseconds);
    }

    /// <summary>
    /// Call this method frequently to make obstacle detection work
    /// This method triggers ultrasonic distance measurement at
    /// predefined measurement intervals and makes the distance
    /// available as the property ObstacleDistance
    /// </summary>
    public void Update()
    {
        // Don't measure at every call because it blocks all processing
        // during the measurement; so use a timer that times out periodically
        if (scanIntervalTimer.Check())
        {
            Robot.LEDs(0, 0, 255); // Flash the green LED on the Romi board
            ObstacleDistance = distanceSensor.GetUltrasoneDistance();
            Console.WriteLine($"New distance: {ObstacleDistance} cm");
            Robot.LEDs(0, 0, 0);
        }
        else
        {
            //Console.WriteLine("Masurement skipped");
        }
    }
}