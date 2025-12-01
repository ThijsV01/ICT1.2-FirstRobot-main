using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
public class DriveSystem : IUpdatable
{
    private double forwardSpeed; // Between -1.0 and 1.0
    private double turnSpeed; // Between -1.0 and 1.0
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
    public double GetSpeed() => forwardSpeed;

    // Use values between -1.0 (reverse) and 1.0 (forward)
    public void SetForwardSpeed(double newSpeed)
    {
        forwardSpeed = newSpeed;
        CalculateRobotMotorSpeeds();
        ControlRobotMotorSpeeds();
    }

    // Use values between 1.0 for clockwise (right hand turn)
    // and -1.0 for counter-clockwise (left hand turn)
    // 0.0 is straight ahead
    public void SetTurnSpeed(double newTurnSpeed)
    {
        turnSpeed = newTurnSpeed;
        CalculateRobotMotorSpeeds();
        ControlRobotMotorSpeeds();
    }
    public void Stop()
    {
        SetForwardSpeed(0.0);
        SetTurnSpeed(0.0);
    }
    // Converts the logical speed value (-1.0 to 1.1) to the value that
    // the robot needs (-300 to 300)
    private short ConvertToRobotSpeedValue(double speed)
    {
        return (short)Math.Clamp(Math.Round(speed * 300.0), -300.0, 300.0);
    }
    private void CalculateRobotMotorSpeeds()
    {
        actualSpeedLeft = Math.Clamp(turnSpeed + forwardSpeed, -1.0, 1.0);
        actualSpeedRight = Math.Clamp(-turnSpeed + forwardSpeed, -1.0, 1.0);
        //Console.WriteLine($"Actual speed  left={actualSpeedLeft,4:F2}   right={actualSpeedRight,4:F2}");
    }
    private void ControlRobotMotorSpeeds()
    {
        short speedLeft = ConvertToRobotSpeedValue(actualSpeedLeft);
        short speedRight = ConvertToRobotSpeedValue(actualSpeedRight);
        //Console.WriteLine($"Motor speed  left: {speedLeft}   right: {speedRight}");
        Robot.Motors(speedLeft, speedRight);
    }
    public void Update()
    {
        // Nothing needed yet

        // TODO Use a separate target speed and an actual speed
        //      and change the actual speed to approach the target speed
        //      in small steps, to make speed changes of the robot more
        //      gradual
    }
}