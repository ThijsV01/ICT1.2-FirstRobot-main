using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();
RobotState robotState = RobotState.Idle;

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem];

const double MaxSpeed = 0.4;
const double SpeedStep = 0.05;

while (true)//Samen gedaan met Julian, dus zullen overeenkomsten tussen zijn.
{
    if (Random.Shared.Next(15) == 12)
    {
        robotState = RobotState.Accelerating;
    }
    Robot.Wait(200);
    foreach (IUpdatable updatable in updatables)
    {
        updatable.Update();
    }

    int obstacleDistance = obstacleDetectionSystem.ObstacleDistance;
    //bool humandetected=irhumandetetectionsystem.havefoundhuman
    //if(humandetected) robotstate idle

    switch (robotState)
    {
        case RobotState.Idle:
            driveSystem.Stop();
            //if(interactietijd==datetimenow)
            //{
            //interaction() zal een loop instaan
            //}else{
            //drivesystem.Stop()
            //}
          Robot.PlayNotes("L16EGC6G6");
          break;

        case RobotState.Accelerating:
            robotState=driveSystem.Accelerate(obstacleDistance,SpeedStep,MaxSpeed);
            break;

        case RobotState.Cruising:
            robotState=driveSystem.Cruise(obstacleDistance);
            break;

        case RobotState.Decelerating:
            robotState=driveSystem.Decelerate(obstacleDistance,SpeedStep);
            break;
    }
    Console.WriteLine($"Robotstate: {robotState} Speed: {driveSystem.GetSpeed()} Distance: {obstacleDistance}");
}