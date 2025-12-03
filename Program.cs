using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;

//----------------------------------------------------------------------------------------
//AANMAKEN VAN ALLE OBJECTEN
//----------------------------------------------------------------------------------------
DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();
IRHumanDetectionSystem irHumanDetectionSystem = new IRHumanDetectionSystem();
InteractionManager interactionManager = new InteractionManager();

LCD16x2 ledScreen=new LCD16x2(0x3E);
Led orangeLed = new Led(22);
Button buttonOrange = new Button(23);
Led blueLed = new Led(5);
Button buttonBlue = new Button(6);
Buzzer buzzer = new Buzzer(12,100);

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem, irHumanDetectionSystem];

//----------------------------------------------------------------------------------------
//CONSTANTEN VOOR IN DE LOOP
//----------------------------------------------------------------------------------------
const double MaxSpeed = 0.5;
const double SpeedStep = 0.05;
const int ObstacleSafeDistance = 20;
const int ObstacleStopDistance = 10;
const double TurnSpeed = 0.75;

//----------------------------------------------------------------------------------------
//ROBOTSTATUS BIJ OPSTARTEN
//----------------------------------------------------------------------------------------
RobotState robotState = RobotState.Idle;

//----------------------------------------------------------------------------------------
//HELP FUNCTIES
//----------------------------------------------------------------------------------------
string lastText="";

void Display(string text)
{
    if (text != lastText)
    {
        lastText=text;
        ledScreen.SetText(text);
    }
}
void IncreaseSpeed()
{
    driveSystem.SetForwardSpeed(Math.Min(driveSystem.GetSpeed() + SpeedStep, MaxSpeed));
}
void DecreaseSpeed()
{
    driveSystem.SetForwardSpeed(Math.Max(driveSystem.GetSpeed() - SpeedStep, 0));
}
void TurnToAvoid()
{
    driveSystem.Stop();
    driveSystem.SetTurnSpeed(TurnSpeed);
    Robot.Wait(150);
    driveSystem.Stop();
}

//----------------------------------------------------------------------------------------
//DE LOOP
//----------------------------------------------------------------------------------------
while (true)
{
    Robot.Wait(50);
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
            Display("IDLE");

            if (interactionManager.IsInteractionTime())
            {
                robotState = RobotState.Driving;
            }

            break;
        case RobotState.Driving:

            Display("DRIVING");
            buzzer.Beep();

            if (humanDetected)
            {
                robotState = RobotState.StoppingForHuman;
                break;
            }

            if (obstacleDistance < ObstacleSafeDistance)
            {
                robotState = RobotState.AvoidingObstacle;
                break;
            }

            IncreaseSpeed();
            break;
        case RobotState.AvoidingObstacle:

            Display("AVOIDING \nOBSTACLE");

            if (driveSystem.GetSpeed() > 0 && obstacleDistance > ObstacleStopDistance)
            {
                DecreaseSpeed();
                break;
            }

            TurnToAvoid();
            robotState = RobotState.Driving;
            break;
        case RobotState.StoppingForHuman:

            Display("AAAH HUMAN!!");

            if (driveSystem.GetSpeed() > 0 && obstacleDistance > ObstacleStopDistance)
            {
                DecreaseSpeed();
                break;
            }
            
            driveSystem.Stop();
            Display("START \nINTERACTION");
            interactionManager.StartActivity();
            robotState = RobotState.Idle;
            break;
    }
    //Console.WriteLine($"Robotstate: {robotState} Speed: {driveSystem.GetSpeed()} Distance: {obstacleDistance}");
}