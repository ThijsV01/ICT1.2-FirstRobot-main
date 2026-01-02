using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
public class DriveSystem : IUpdatable
{
    private double forwardSpeedTarget; // desired speed
    private double turnSpeedTarget;    // desired turn
    private double forwardSpeedActual; // Between -1.0 and 1.0
    private double turnSpeedActual; // Between -1.0 and 1.0
    private double actualSpeedLeft;  // Between -1.0 and 1.0, where 0.0 is stop
    private double actualSpeedRight; // Between -1.0 and 1.0, where 0.0 is stop
    private const double AccelerationStep = 0.05; // smooth acceleration
    private const double BrakeStep = 0.10;

    public DriveSystem()
    {
        Console.WriteLine("DriveSystem constructor called");
        // Start off stationary
        forwardSpeedActual = 0.0;
        turnSpeedActual = 0.0;
        forwardSpeedTarget = 0.0;
        turnSpeedTarget = 0.0;
        CalculateRobotMotorSpeeds(); // Updates actualSpeedLeft and actualSpeedRight
    }
    public double GetSpeed() => forwardSpeedActual;

    // Use values between -1.0 (reverse) and 1.0 (forward)
    public void SetForwardSpeed(double newSpeed)
    {
        forwardSpeedTarget = Math.Clamp(newSpeed, -1.0, 1.0);
    }

    // Use values between 1.0 for clockwise (right hand turn)
    // and -1.0 for counter-clockwise (left hand turn)
    // 0.0 is straight ahead
    public void SetTurnSpeed(double newTurnSpeed)
    {
        turnSpeedTarget = Math.Clamp(newTurnSpeed, -1.0, 1.0);
    }
    private double ActualToTarget(double actual, double target, double speedStep)
    {
        if (actual < target)
        {
            return Math.Min(actual + speedStep, target);
        }
        if (actual > target)
        {
            return Math.Max(actual - speedStep, target);
        }
        return actual;
    }
    public void Stop()
    {
        SetForwardSpeed(0.0);
        SetTurnSpeed(0.0);
    }
    public void StopImmediately()
    {
        forwardSpeedActual = 0.0;
        turnSpeedActual = 0.0;
        forwardSpeedTarget = 0.0;
        turnSpeedTarget = 0.0;

        CalculateRobotMotorSpeeds();
        ControlRobotMotorSpeeds();
    }
    // Converts the logical speed value (-1.0 to 1.1) to the value that
    // the robot needs (-300 to 300)
    private short ConvertToRobotSpeedValue(double speed)
    {
        return (short)Math.Clamp(Math.Round(speed * 300.0), -300.0, 300.0);
    }
    private void CalculateRobotMotorSpeeds()
    {
        actualSpeedLeft = Math.Clamp(turnSpeedActual + forwardSpeedActual, -1.0, 1.0);
        actualSpeedRight = Math.Clamp(-turnSpeedActual + forwardSpeedActual, -1.0, 1.0);
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
        double forwardStep = AccelerationStep;
        double turnStep = AccelerationStep;

        // If we are slowing down → brake faster
        if (Math.Abs(forwardSpeedTarget) < Math.Abs(forwardSpeedActual))
        {
            forwardStep = BrakeStep;
        }

        if (Math.Abs(turnSpeedTarget) < Math.Abs(turnSpeedActual))
        {
            turnStep = BrakeStep;
        }

        // Move actual values toward targets
        forwardSpeedActual = ActualToTarget(forwardSpeedActual, forwardSpeedTarget, forwardStep);
        turnSpeedActual = ActualToTarget(turnSpeedActual, turnSpeedTarget, turnStep);

        CalculateRobotMotorSpeeds();
        ControlRobotMotorSpeeds();
    }
}