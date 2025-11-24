using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

/// <summary>
/// This class controls the movements of the robot
/// </summary>
public class DriveSystem : IUpdatable
{
    // Speeds independent of what wheel configuration is used
    // so there is a value for forward (and reverse) and a value for turning
    // Note: we could have made these C# properties as an alternative to creating
    //       SetForwardSpeed() and SetTurnSpeed(), to achieve the same result
    private double forwardSpeed; // Between -1.0 and 1.0
    private double turnSpeed; // Between -1.0 and 1.0

    // Speeds for the left and right wheels
    private double actualSpeedLeft;  // Between -1.0 and 1.0, where 0.0 is stop
    private double actualSpeedRight; // Between -1.0 and 1.0, where 0.0 is stop

    public DriveSystem()
    {
        Console.WriteLine("DriveSystem constructor called");
        // Start off stationary
        forwardSpeed = 0.0;
        turnSpeed = 0.0;
        CalculateRobotMotorSpeeds(); // Updates actualSpeedLeft and actualSpeedRight
    }

    /// <summary>
    /// Return the current forward speed of the robot
    /// </summary>
    /// <returns>The logical speed, where -1.0 is full speed reverse, 0.0 is stop,
    /// and 1.0 is full speed forward</returns>
    public double GetSpeed() => forwardSpeed;

    /// <summary>
    /// Set the speed of the robot and make it move
    /// Use values between -1.0 (reverse) and 1.0 (forward)
    /// </summary>
    /// <param name="speed">A logical speed value between -1.0 (full speed reverse)
    /// and 1.0 (full speed forward), 0.0 means stop</param>
    public void SetForwardSpeed(double newSpeed)
    {
        forwardSpeed = newSpeed;
        CalculateRobotMotorSpeeds();
        ControlRobotMotorSpeeds();
    }

    /// <summary>
    /// Set the speed of turning the robot and make the robot move
    /// Use values between 1.0 for clockwise (right hand turn)
    /// and -1.0 for counter-clockwise (left hand turn)
    /// 0.0 is straight ahead
    /// </summary>
    /// <param name="newTurnSpeed">A logical turn speed between -1.0
    /// (full speed counterclockwise) and 1.0 (full speed clockwise)</param>
    public void SetTurnSpeed(double newTurnSpeed)
    {
        turnSpeed = newTurnSpeed;
        CalculateRobotMotorSpeeds();
        ControlRobotMotorSpeeds();
    }

    /// <summary>
    /// Set the speed of the robot to zero to stop the robot
    /// </summary>
    public void Stop()
    {
        SetForwardSpeed(0.0);
        SetTurnSpeed(0.0);
    }

    /// <summary>
    /// Converts the logical speed value (-1.0 to 1.1) to the value that
    /// the robot needs (-300 to 300)
    /// Note that 300 is approximately the maximum for the Romi robot motors
    /// </summary>
    /// <param name="speed">Logical speed value, should be between -1.0 and 1.0</param>
    /// <returns>Robot motor speed value</returns>
    private short ConvertToRobotSpeedValue(double speed)
    {
        return (short) Math.Clamp(Math.Round(speed * 300.0), -300.0, 300.0);
    }

    /// <summary>
    /// Calculates actual speed values for the left and right motor
    /// by combining forward speed and turn speed
    /// </summary>
    private void CalculateRobotMotorSpeeds()
    {
        // Calculate speed for the left and right motors
        // Clockwise rotation (turn speed > 0.0) means that the left motor
        // moves faster forward than the right motor, so the turn speed
        // is added for the left motor and subtracted for the right motor
        actualSpeedLeft  = Math.Clamp( turnSpeed + forwardSpeed, -1.0, 1.0);
        actualSpeedRight = Math.Clamp(-turnSpeed + forwardSpeed, -1.0, 1.0);
        Console.WriteLine($"Actual speed  left={actualSpeedLeft,4:F2}   right={actualSpeedRight,4:F2}");
    }

    /// <summary>
    /// Sends the actual speed to both robot motors
    /// </summary>
    private void ControlRobotMotorSpeeds()
    {
        short speedLeft  = ConvertToRobotSpeedValue(actualSpeedLeft);
        short speedRight = ConvertToRobotSpeedValue(actualSpeedRight);
        // Show the motor speed values on the console
        Console.WriteLine($"Motor speed  left: {speedLeft}   right: {speedRight}");
        // Change the motor speeds on the robot itself
        Robot.Motors(speedLeft, speedRight);
    }

    /// <summary>
    /// This method should be called frequently
    /// </summary>
    public void Update()
    {
        // Nothing needed yet

        // TODO Use a separate target speed and an actual speed
        //      and change the actual speed to approach the target speed
        //      in small steps, to make speed changes of the robot more
        //      gradual
    }
}