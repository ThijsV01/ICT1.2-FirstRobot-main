using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();
IRHumanDetectionSystem irHumanDetectionSystem = new IRHumanDetectionSystem();
InteractionManager interactionManager = new InteractionManager();

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem, irHumanDetectionSystem];

const double MaxSpeed = 0.4;
const double SpeedStep = 0.05;
bool humanDetectedDuringSearch = false;

RobotState robotState = RobotState.Idle;

while (true)
{
    Robot.Wait(200);
    foreach (IUpdatable updatable in updatables)
    {
        updatable.Update();
    }

    int obstacleDistance = obstacleDetectionSystem.ObstacleDistance;
    int humanDetected = irHumanDetectionSystem.FoundHuman;

    switch (robotState)
    {
        case RobotState.Idle:
            driveSystem.Stop();
            //kijk of er een interactiemoment aan komt
            if (interactionManager.IsInteractionTime() && !humanDetectedDuringSearch)
            {
                robotState = RobotState.Accelerating;
            }
            else if (humanDetectedDuringSearch)
            {
                interactionManager.StartActivity();
                humanDetectedDuringSearch = false;
            }
            else
            {
                driveSystem.SetTurnSpeed(0.75);
                Robot.Wait(150);
                driveSystem.Stop();
                robotState = RobotState.Accelerating;
            }
            break;
        case RobotState.Accelerating:
            if (humanDetected == 1)
            {
                robotState = RobotState.Decelerating;
                humanDetectedDuringSearch = true;
            }
            if (obstacleDistance < 20)
            {
                robotState = RobotState.Decelerating;
            }
            else
            {
                driveSystem.SetForwardSpeed(Math.Min(driveSystem.GetSpeed() + SpeedStep, MaxSpeed));
                if (driveSystem.GetSpeed() >= MaxSpeed)
                {
                    robotState = RobotState.Cruising;
                }
            }
            break;
        case RobotState.Cruising:
            if (humanDetected == 1)
            {
                robotState = RobotState.Decelerating;
                humanDetectedDuringSearch = true;
            }
            else if (obstacleDistance < 20)
            {
                robotState = RobotState.Decelerating;
            }
            else
            {
                driveSystem.SetForwardSpeed(MaxSpeed);
            }
            break;
        case RobotState.Decelerating:

            if (humanDetected == 1)
            {
                humanDetectedDuringSearch = true;
            }
            if (humanDetectedDuringSearch)
            {
                if (driveSystem.GetSpeed() > 0.05 && obstacleDistance > 10)
                {
                    driveSystem.SetForwardSpeed(Math.Max(driveSystem.GetSpeed() - SpeedStep, 0));
                }
                else
                {
                    robotState = RobotState.Idle;
                }
            }
            else if (obstacleDistance < 10 || driveSystem.GetSpeed() <= 0)
            {
                robotState=RobotState.Idle;
            }
            else if (obstacleDistance >= 20)
            {
                robotState = RobotState.Accelerating;
            }
            break;
    }
    Console.WriteLine($"Robotstate: {robotState} Speed: {driveSystem.GetSpeed()} Distance: {obstacleDistance}");
}