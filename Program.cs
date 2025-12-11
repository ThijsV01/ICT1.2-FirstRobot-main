using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using SimpleMqtt;
using HiveMQtt;
using System.Text;

//----------------------------------------------------------------------------------------
//MQTT CLIENT AANMAKEN
//----------------------------------------------------------------------------------------

string clientId = "Robot-" + Guid.NewGuid().ToString();
var mqttClient = SimpleMqttClient.CreateSimpleMqttClientForHiveMQ(clientId);

await mqttClient.SubscribeToTopic("robot/2242722/command/#");
await mqttClient.SubscribeToTopic("robot/2242722/interactionmoment");

//----------------------------------------------------------------------------------------
//AANMAKEN VAN ALLE OBJECTEN
//----------------------------------------------------------------------------------------
DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();
IRHumanDetectionSystem irHumanDetectionSystem = new IRHumanDetectionSystem();
InteractionManager interactionManager = new InteractionManager(mqttClient);

LCD16x2 ledScreen = new LCD16x2(0x3E);
Led orangeLed = new Led(22);
Button buttonOrange = new Button(23);
Led blueLed = new Led(5);
Button buttonBlue = new Button(6);
Buzzer buzzer = new Buzzer(12, 100);

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem, irHumanDetectionSystem];
DateTime lastUpdate = DateTime.MinValue;
bool interactionMoment=false;

//----------------------------------------------------------------------------------------
//CONSTANTEN VOOR IN DE LOOP
//----------------------------------------------------------------------------------------
const double MaxSpeed = 0.4;
const double SpeedStep = 0.10;
const int ObstacleSafeDistance = 40;
const int ObstacleStopDistance = 10;
const double TurnSpeed = 0.75;

//----------------------------------------------------------------------------------------
//ROBOTSTATUS BIJ OPSTARTEN
//----------------------------------------------------------------------------------------
RobotState robotState = RobotState.Idle;

//----------------------------------------------------------------------------------------
//HELP FUNCTIES
//----------------------------------------------------------------------------------------
string lastText = "";

void Display(string text)
{
    if (text != lastText)
    {
        lastText = text;
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
//MQTT BIJ ONTVANGEN COMMANDO WORDT DIT UITGEVOERD
//----------------------------------------------------------------------------------------
mqttClient.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine($"Topic: {args.Topic} Message: {args.Message}");

    if (args.Topic == "robot/2242722/command/start")
    {
        robotState=RobotState.Idle;
    }

    if (args.Topic == "robot/2242722/command/stop")
    {
        robotState=RobotState.Offline;
    }
    if (args.Topic == "robot/2242722/interactionmoment")
    {
        interactionMoment=true;
    }
};

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
    
    //om ervoor te zorgen dat ik niet te veel dezelfde publish krijg die niet nodig zijn.
    if ((DateTime.Now - lastUpdate).TotalMinutes >= 1 ||lastUpdate==DateTime.MinValue)
    {
        string payloadBattery = $"{1};{Robot.ReadBatteryMillivolts()}";
        await mqttClient.PublishMessage(payloadBattery, "robot/2242722/battery");
        string payloadObsDist = $"{obstacleDistance}";
        await mqttClient.PublishMessage(payloadObsDist, "robot/2242722/sensor/obstacledistance");
        string payloadHumanDetected = $"{(humanDetected ? 1 : 0)}";
        await mqttClient.PublishMessage(payloadHumanDetected, "robot/2242722/sensor/humandetected");
        string payloadRobotState = $"{1};{robotState}";
        //await mqttClient.PublishMessage(payloadRobotState, "robot/2242722/state");
        lastUpdate = DateTime.Now;
    }
    
    switch (robotState)
    {
        case RobotState.Idle:

            driveSystem.Stop();
            Display("IDLE");

            if (interactionMoment)
            {
                interactionMoment=false;
                robotState = RobotState.Driving;
            }

            break;
        case RobotState.Driving:

            Display("DRIVING");

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
        case RobotState.Offline:
             Display("OFFLINE");
            break;
    }
    //Console.WriteLine($"Robotstate: {robotState} Speed: {driveSystem.GetSpeed()} Distance: {obstacleDistance}");
}