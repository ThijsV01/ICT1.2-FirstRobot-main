using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using SimpleMqtt;
public class InteractionSystem : IUpdatable
{
    private readonly SimpleMqttClient _mqttClient;
    private readonly LCD16x2 ledScreen = new LCD16x2(0x3E);
    private readonly Led orangeLed = new Led(22);
    private readonly Button buttonOrange = new Button(23);
    private readonly Led blueLed = new Led(5);
    private readonly Button buttonBlue = new Button(6);
    private readonly Buzzer buzzer = new Buzzer(12, 100);
    private InteractionState state = InteractionState.None;
    private DateTime interactionStart;
    public bool IsActive => state != InteractionState.None;
    private IInteractionGame? currentGame;

    private const int InteractionTimeoutMinutes = 5;
    public InteractionSystem(SimpleMqttClient client)
    {
        _mqttClient = client;
    }
    public void StartInteraction()
    {
        interactionStart = DateTime.Now;
        state = InteractionState.ChoosingActivity;
        string payloadStartInteraction = $"{interactionStart.ToString("dd-MM-yyyy")};{interactionStart.ToString("hh:mm:ss")}";
        _mqttClient.PublishMessage(payloadStartInteraction, "robot/2242722/interaction/start");
        Robot.PlayNotes("L16EGC6G6");
        ledScreen.SetText("CHOOSE \nACTIVITY");
    }
    public void EndInteraction()
    {
        state = InteractionState.None;
        currentGame = null;
        string payloadEndInteraction = $"{DateTime.Now.ToString("dd-MM-yyyy")};{DateTime.Now.ToString("hh:mm:ss")}";
        _mqttClient.PublishMessage(payloadEndInteraction, "robot/2242722/interaction/eind");
        Robot.PlayNotes("L16EGC6G6");
        ledScreen.SetText("");
    }
    private void HandleChoosing()
    {
        if (buttonBlue.GetState() == "Pressed")
        {
            _mqttClient.PublishMessage("SimonSays", "robot/2242722/interaction/soort");

            currentGame = new SimonSays();
            currentGame.StartGame();

            state = InteractionState.Playing;
        }
        else if (buttonOrange.GetState() == "Pressed")
        {
            _mqttClient.PublishMessage("Quiz", "robot/2242722/interaction/soort");

            currentGame = new Quiz();
            currentGame.StartGame();

            state = InteractionState.Playing;
        }
        //meer games maken kan dus als ik hier zou uitbreiden
    }
    public void Update()
    {
        if (state == InteractionState.None)
        {
            return;
        }
        if ((DateTime.Now - interactionStart).TotalMinutes > InteractionTimeoutMinutes)
        {
            _mqttClient.PublishMessage("No Interaction", "robot/2242722/interaction/soort");
            ledScreen.SetText("NO \nINTERACTION");
            EndInteraction();

            return;
        }
        switch (state)
        {
            case InteractionState.ChoosingActivity:
                HandleChoosing();
                break;
            case InteractionState.Playing:
                currentGame!.Update();
                if (currentGame.IsFinished)
                {
                    EndInteraction();
                }
                break;
        }
    }
}
