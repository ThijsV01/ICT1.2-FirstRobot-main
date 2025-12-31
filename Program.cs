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
LCD16x2 ledScreen = new LCD16x2(0x3E);
Button buttonOrange = new Button(23);
Button buttonBlue = new Button(6);
Button buttonRed = new Button(17);

DriveSystem driveSystem = new DriveSystem();
ObstacleDetectionSystem obstacleDetectionSystem = new ObstacleDetectionSystem();
IRHumanDetectionSystem irHumanDetectionSystem = new IRHumanDetectionSystem();
InteractionSystem interactionSystem = new InteractionSystem(mqttClient, ledScreen, buttonOrange, buttonBlue, buttonRed);

List<IUpdatable> updatables = [obstacleDetectionSystem, driveSystem, irHumanDetectionSystem, interactionSystem];
DateTime lastUpdate = DateTime.MinValue;
bool interactionMoment = false;

//----------------------------------------------------------------------------------------
//CONSTANTEN VOOR IN DE LOOP
//----------------------------------------------------------------------------------------
const double MaxSpeed = 0.4;
const double SpeedStep = 0.10;
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
async Task TurnToAvoid()
{
    driveSystem.Stop();
    driveSystem.SetTurnSpeed(TurnSpeed);
    await Task.Delay(150);
    driveSystem.Stop();
}
Button? GetPressedButton()
{
    if (buttonRed.GetState() == "Pressed")
    {
        return buttonRed;
    }
    if (buttonOrange.GetState() == "Pressed")
    {
        return buttonOrange;
    }
    if (buttonBlue.GetState() == "Pressed")
    {
        return buttonBlue;
    }
    return null;
}
//----------------------------------------------------------------------------------------
//MQTT BIJ ONTVANGEN BERICHT WORDT DIT UITGEVOERD
//----------------------------------------------------------------------------------------
mqttClient.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine($"Topic: {args.Topic} Message: {args.Message}");

    if (args.Topic == "robot/2242722/command/start")
    {
        robotState = RobotState.Idle;
    }

    if (args.Topic == "robot/2242722/command/stop")
    {
        robotState = RobotState.Offline;
    }
    if (args.Topic == "robot/2242722/interactionmoment")
    {
        interactionMoment = true;
    }
};

//----------------------------------------------------------------------------------------
//DE LOOP
//----------------------------------------------------------------------------------------
while (true)
{
    await Task.Delay(50);
    foreach (IUpdatable updatable in updatables)
    {
        updatable.Update();
    }
    int obstacleDistance = obstacleDetectionSystem.ObstacleDistance;
    bool humanDetected = irHumanDetectionSystem.FoundHuman == 1;

    //om ervoor te zorgen dat ik niet te veel dezelfde publish krijg die niet nodig zijn.
    if ((DateTime.Now - lastUpdate).TotalMinutes >= 1 || lastUpdate == DateTime.MinValue)
    {
        string payloadBattery = $"{1};{Robot.ReadBatteryMillivolts()}";
        await mqttClient.PublishMessage(payloadBattery, "robot/2242722/battery");
        string payloadObsDist = $"{obstacleDistance}";
        await mqttClient.PublishMessage(payloadObsDist, "robot/2242722/sensor/obstacledistance");

        lastUpdate = DateTime.Now;
    }
    //alleen bij mens detecteren in database zetten, want wat heb je eraan als je het erin zet als er niks gebeurd.
    if (humanDetected)
    {
        string payloadHumanDetected = $"{(humanDetected ? 1 : 0)}";
        await mqttClient.PublishMessage(payloadHumanDetected, "robot/2242722/sensor/humandetected");
    }
    if (robotState != RobotState.Interacting && robotState != RobotState.Idle && robotState != RobotState.Offline)
    {
        if (GetPressedButton() != null)
        {
            robotState = RobotState.Offline;
        }
    }

    switch (robotState)
    {
        case RobotState.Idle:

            driveSystem.Stop();
            Display("IDLE");

            if (interactionMoment)
            {
                interactionMoment = false;
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

            await TurnToAvoid();
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
            interactionSystem.StartInteraction();
            robotState = RobotState.Interacting;
            break;
        case RobotState.Interacting:

            if (!interactionSystem.IsActive)
            {
                robotState = RobotState.Idle;
            }
            break;
        case RobotState.Offline:

            driveSystem.Stop();
            await Task.Delay(500);
            var pressedButton = GetPressedButton();
            if (pressedButton != null)
            {
                robotState = RobotState.Idle;
            }
            Display("OFFLINE");
            break;
    }
}
