using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

Console.WriteLine("Hello First Robot!");
Robot.PlayNotes("g>g");

Led led = new Led(22);
bool ledIsOn = false;

DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();
RobotState robotState = RobotState.Idle;

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem];
bool programRuns = true;
double maxSpeed = 0.4;
double step = 0.05;

while (true)
{
    if (programRuns)
    {
        if (Random.Shared.Next(15) == 12)
        {
            robotState = RobotState.Accelerating;
            programRuns = true;
        }
        Robot.Wait(200);
        foreach (IUpdatable updatable in updatables)
        {
            updatable.Update();
        }

        int obstacleDistance = obstacleDetectionSystem.ObstacleDistance;
        Console.WriteLine($"Obstacle distance = {obstacleDistance} cm");
        double speed = driveSystem.GetSpeed();
        double changedSpeed = speed + step;
        double decelSpeed = speed - step;
        switch (robotState)
        {
            case RobotState.Idle:
                driveSystem.Stop();
                break;
            case RobotState.Accelerating:
                if (obstacleDistance >= 50)
                {
                    if (changedSpeed < maxSpeed)
                    {
                        driveSystem.SetForwardSpeed(changedSpeed);
                    }
                    else
                    {
                        driveSystem.SetForwardSpeed(maxSpeed);
                        robotState = RobotState.Cruising;
                    }
                }
                else
                {
                    robotState = RobotState.Decelerating;
                }

                break;
            case RobotState.Cruising:
                if (obstacleDistance < 50)
                {
                    robotState = RobotState.Decelerating;
                }
                break;
            case RobotState.Decelerating:
                if (obstacleDistance < 5)
                {
                    robotState = RobotState.Idle;
                }
                else
                {
                    if (decelSpeed < 0)
                    {
                        decelSpeed = 0;
                        robotState = RobotState.Idle;
                    }
                    driveSystem.SetForwardSpeed(decelSpeed);
                }
                break;
        }
    }
    // if (obstacleDistance < 5)
    // {
    //     // If the obstacle comes closer,
    //     // then reverse while turning left
    //     driveSystem.SetForwardSpeed(-0.15);
    //     driveSystem.SetTurnSpeed(-0.3);
    // }
    // else if (obstacleDistance < 8)
    // {
    //     // Avoid a collision
    //     driveSystem.Stop();
    // }
    // else if (obstacleDistance < 12)
    // {
    //     // Slow down more
    //     driveSystem.SetForwardSpeed(0.10);
    //     driveSystem.SetTurnSpeed(0.0);
    // }
    // else if (obstacleDistance < 30)
    // {
    //     // Slow down
    //     driveSystem.SetForwardSpeed(0.19);
    //     driveSystem.SetTurnSpeed(0.0);
    // }
    // else
    // {
    //     // Run forward in a left turn
    //     driveSystem.SetForwardSpeed(0.25);
    //     driveSystem.SetTurnSpeed(0.00);
    // }

    // Blink the LED each time we pass through this event loop
    ledIsOn = !ledIsOn;
    if (ledIsOn)
    {
        led.SetOn();
    }
    else
    {
        led.SetOff();
    }
}