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
    private SimonSays? simonSays;
    private Quiz? quiz;

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
        simonSays = null;
        quiz = null;
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

            simonSays = new SimonSays();
            simonSays.StartGame();

            state = InteractionState.SimonSays;
        }
        else if (buttonOrange.GetState() == "Pressed")
        {
            _mqttClient.PublishMessage("Quiz", "robot/2242722/interaction/soort");

            quiz = new Quiz();
            quiz.StartGame();

            state = InteractionState.Quiz;
        }
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
            
            return;
        }
        switch (state)
        {
            case InteractionState.ChoosingActivity:
                HandleChoosing();
                break;
            case InteractionState.Quiz:
                //hier moet dan de quiz die dus werkt door het feit dat de robotstate interaction deze functie blijft aanroepen
                //aan het einde wanneer interactie voorbij is moet de state terug naar none (endinteraction methode aanroepen)
                break;
            case InteractionState.SimonSays:
                //hier moet dan de simon says die dus werkt door het feit dat de robotstate interaction deze functie blijft aanroepen
                //aan het einde wanneer interactie voorbij is moet de state terug naar none (endinteraction methode aanroepen)
                break;
        }
    }
}
