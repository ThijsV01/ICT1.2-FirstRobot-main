using System.Text.Json;
using System.Threading.Tasks;
using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using SimpleMqtt;
public class InteractionSystem : IUpdatable
{
    private readonly SimpleMqttClient _mqttClient;
    private LCD16x2 _ledScreen;
    private readonly Led orangeLed = new Led(22);
    private readonly Button buttonOrange;
    private readonly Led blueLed = new Led(5);
    private readonly Button buttonBlue;
    private readonly Led redLed = new Led(16);
    private readonly Button buttonRed;
    private InteractionState state = InteractionState.None;
    private DateTime interactionStart;
    public bool IsActive => state != InteractionState.None;
    private IInteractionGame? currentGame;
    private IInteractionGame? nextGame;
    private GameResult currentResult = new();
    private const int InteractionTimeoutMinutes = 5;
    private DateTime startDelayUntil;
    private DateTime lastButtonPress = DateTime.MinValue;
    private const int DebounceMs = 250;
    public InteractionSystem(SimpleMqttClient client, LCD16x2 ledScreen, Button orange, Button blue, Button red)
    {
        _mqttClient = client;
        _ledScreen = ledScreen;
        buttonOrange = orange;
        buttonBlue = blue;
        buttonRed = red;

    }
    public void StartInteraction()
    {
        interactionStart = DateTime.Now;
        currentResult.StartTime = interactionStart;
        currentResult.Date = DateTime.Now.Date;
        state = InteractionState.ChoosingActivity;
        Robot.PlayNotes("L16EGC6G6");
        _ledScreen.SetText("CHOOSE \nACTIVITY");
    }
    public void EndInteraction()
    {
        state = InteractionState.None;
        SetResultValues();
        currentGame = null;
        _ = _mqttClient.PublishMessage(JsonSerializer.Serialize(currentResult), "robot/2242722/activities/endofinteraction");
        currentResult = new();
        Robot.PlayNotes("L16EGC6G6");
    }
    private void HandleChoosing()
    {
        if ((DateTime.Now - lastButtonPress).TotalMilliseconds < DebounceMs)
        {
            return;
        }
    
        if (buttonBlue.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            PrepareGame("Simon Says",new SimonSays(buttonRed, buttonOrange, buttonBlue, _ledScreen,orangeLed, blueLed, redLed));
        }
        else if (buttonOrange.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            PrepareGame("Quiz",new Quiz(_mqttClient, buttonRed, buttonOrange, buttonBlue, _ledScreen));
        }
        else if (buttonRed.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            PrepareGame("Reaction Game",new ReactionGame(buttonRed, buttonOrange, buttonBlue, _ledScreen,orangeLed, blueLed, redLed));
        }
    }
    private void PrepareGame(string name, IInteractionGame game)
    {
        _ledScreen.SetText(name);
        currentResult.KindOfGame = name;

        nextGame = game;
        startDelayUntil = DateTime.Now.AddMilliseconds(300);

        Robot.PlayNotes("L16EGC6G6");

        state = InteractionState.StartingActivity;
    }
    private void SetResultValues()
    {
        currentResult.RobotId = 1;
        currentResult.EndTime = DateTime.Now;
        if (currentGame == null)
        {
            currentResult.InteractionState = "Not started";
            currentResult.AverageReactionTimeMs = -1;
        }
        else
        {
            currentResult.SimonSaysAmount = currentGame.Result.SimonSaysAmount;
            currentResult.InteractionState = currentGame.Result.InteractionState;
            currentResult.AverageReactionTimeMs = currentGame.Result.AverageReactionTimeMs;
            currentResult.CorrectlyAnsweredPercentage = currentGame.Result.CorrectlyAnsweredPercentage;
        }
    }
    public void Update()
    {
        if (state == InteractionState.None)
        {
            return;
        }
        if ((DateTime.Now - interactionStart).TotalMinutes > InteractionTimeoutMinutes && state == InteractionState.ChoosingActivity)
        {
            _ledScreen.SetText("NO \nINTERACTION");
            currentResult.KindOfGame = "No game chosen";
            EndInteraction();
            return;
        }
        switch (state)
        {
            case InteractionState.ChoosingActivity:
                HandleChoosing();

                break;
            case InteractionState.StartingActivity:
                if (DateTime.Now < startDelayUntil)
                {
                    return;
                }
                currentGame = nextGame!;
                nextGame = null;
                currentGame.StartGame();
                state = InteractionState.Playing;

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
