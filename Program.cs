using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();
IRHumanDetectionSystem irHumanDetectionSystem = new IRHumanDetectionSystem();
InteractionManager interactionManager = new InteractionManager();

LCD16x2 ledScreen=new LCD16x2(0x3E);
Led orangeLed = new Led(22);
Button button = new Button(23);

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem, irHumanDetectionSystem];

const double MaxSpeed = 0.5;
const double SpeedStep = 0.05;

RobotState robotState = RobotState.Idle;

while (true)
{
    Robot.Wait(200);
    foreach (IUpdatable updatable in updatables)
    {
        updatable.Update();
    }

    int obstacleDistance = obstacleDetectionSystem.ObstacleDistance;
    bool humanDetected = irHumanDetectionSystem.FoundHuman == 1;

    switch (robotState)
    {
        case RobotState.Idle:

            driveSystem.Stop();
            ledScreen.SetText("IDLE");

            if (interactionManager.IsInteractionTime())
            {
                robotState = RobotState.Driving;
            }
            break;
        case RobotState.Driving:

            ledScreen.SetText("DRIVING");

            if (humanDetected)
            {
                robotState = RobotState.StoppingForHuman;
                break;
            }
            if (obstacleDistance < 20)
            {
                robotState = RobotState.AvoidingObstacle;
                break;
            }
            driveSystem.SetForwardSpeed(Math.Min(driveSystem.GetSpeed() + SpeedStep, MaxSpeed));
            break;
        case RobotState.AvoidingObstacle:

            ledScreen.SetText("AVOIDING \nOBSTACLE");

            if (driveSystem.GetSpeed() > 0 && obstacleDistance > 10)
            {
                driveSystem.SetForwardSpeed(Math.Max(driveSystem.GetSpeed() - SpeedStep, 0));
                break;
            }

            driveSystem.Stop();
            driveSystem.SetTurnSpeed(0.75);
            Robot.Wait(150);
            driveSystem.Stop();
            robotState = RobotState.Driving;

            break;
        case RobotState.StoppingForHuman:

            ledScreen.SetText("AAAH HUMAN!!");
            orangeLed.SetOn();

            if (driveSystem.GetSpeed() > 0 && obstacleDistance > 10)
            {
                driveSystem.SetForwardSpeed(Math.Max(driveSystem.GetSpeed() - SpeedStep, 0));
                break;
            }
            
            driveSystem.Stop();
            ledScreen.SetText("START \nINTERACTION");
            interactionManager.StartActivity(DateTime.Now);
            orangeLed.SetOff();
            robotState = RobotState.Idle;
            break;
    }
    //Console.WriteLine($"Robotstate: {robotState} Speed: {driveSystem.GetSpeed()} Distance: {obstacleDistance}");
}