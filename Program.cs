using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

Console.WriteLine("Hello First Robot!");
Robot.PlayNotes("g>g");

Led led = new Led(22);
bool ledIsOn = false;

DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem];

while (true)
{
    Robot.Wait(200);
    foreach (IUpdatable updatable in updatables)
    {
        updatable.Update();
    }

    int obstacleDistance = obstacleDetectionSystem.ObstacleDistance;
    Console.WriteLine($"Obstacle distance = {obstacleDistance} cm");

    if (obstacleDistance < 5)
    {
        // If the obstacle comes closer,
        // then reverse while turning left
        driveSystem.SetForwardSpeed(-0.15);
        driveSystem.SetTurnSpeed(-0.3);
    }
    else if (obstacleDistance < 8)
    {
        // Avoid a collision
        driveSystem.Stop();
    }
    else if (obstacleDistance < 12)
    {
        // Slow down more
        driveSystem.SetForwardSpeed(0.10);
        driveSystem.SetTurnSpeed(0.0);
    }
    else if (obstacleDistance < 30)
    {
        // Slow down
        driveSystem.SetForwardSpeed(0.19);
        driveSystem.SetTurnSpeed(0.0);
    }
    else
    {
        // Run forward in a left turn
        driveSystem.SetForwardSpeed(0.25);
        driveSystem.SetTurnSpeed(-0.10);
    }

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