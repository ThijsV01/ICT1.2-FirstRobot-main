using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();
IRHumanDetectionSystem irHumanDetectionSystem = new IRHumanDetectionSystem();

RobotState robotState = RobotState.Idle;

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem, irHumanDetectionSystem];

const double MaxSpeed = 0.4;
const double SpeedStep = 0.05;
bool humanDetectedDuringSearch = false;

//om de interactietijden op te zetten en te verifieren of deze al geweest zijn
List<TimeSpan> interactionTimes =
[
    new TimeSpan(9, 0, 0),
    new TimeSpan(15, 0, 0),
    new TimeSpan(18, 0, 0),
    new TimeSpan(21, 0, 0)
];
List<TimeSpan> completedTimes = [];

while (true)
{
    Robot.Wait(200);
    foreach (IUpdatable updatable in updatables)
    {
        updatable.Update();
    }

    int obstacleDistance = obstacleDetectionSystem.ObstacleDistance;
    int humanDetected = irHumanDetectionSystem.FoundHuman;

    TimeSpan now = DateTime.Now.TimeOfDay;

    //kijk of er een interactiemoment aan komt
    foreach (TimeSpan interactionTime in interactionTimes)
    {
        if (now >= interactionTime && !completedTimes.Contains(interactionTime))
        {
            //interactiemoment komt, laat robot beginnen te rijden
            completedTimes.Add(interactionTime);
            robotState = RobotState.Accelerating;
        }
    }
    //lijst met completed times leeghalen, zodat de volgende dag wel weer alles wordt uitgevoerd.
    if (now.TotalMinutes < 1)
    {
        completedTimes.Clear();
    }

    switch (robotState)
    {
        case RobotState.Idle:
            if (humanDetectedDuringSearch)
            {
                Interaction interaction = new Interaction();
                interaction.StartActivity();
                humanDetectedDuringSearch = false;
            }
            break;
        case RobotState.Accelerating:
            if (humanDetected == 1)
            {
                robotState = RobotState.Decelerating;
                humanDetectedDuringSearch = true;
            }
            else
            {
                robotState = driveSystem.Accelerate(obstacleDistance, SpeedStep, MaxSpeed);
            }
            break;
        case RobotState.Cruising:
            if (humanDetected == 1)
            {
                robotState = RobotState.Decelerating;
                humanDetectedDuringSearch = true;
            }
            else
            {
                robotState = driveSystem.Cruise(obstacleDistance);
            }
            break;
        case RobotState.Decelerating:

            if (humanDetected == 1)
            {
                humanDetectedDuringSearch = true;
            }
            robotState = driveSystem.Decelerate(obstacleDistance, SpeedStep, humanDetectedDuringSearch);
            break;
    }
    Console.WriteLine($"Robotstate: {robotState} Speed: {driveSystem.GetSpeed()} Distance: {obstacleDistance}");
}