using System.Text.Json;
using System.Threading.Tasks;
using Avans.StatisticalRobot;
using Avans.StatisticalRobot.Interfaces;
using SimpleMqtt;
public class InteractionSystem : IUpdatable
{
    private readonly SimpleMqttClient _mqttClient;
    private LCD16x2 _ledScreen;
    private readonly Led orangeLed;
    private readonly Button buttonOrange;
    private readonly Led blueLed;
    private readonly Button buttonBlue;
    private readonly Led redLed;
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
    public InteractionSystem(SimpleMqttClient client, LCD16x2 ledScreen, Button orange, Button blue, Button red, Led orangeLed, Led blueLed, Led redLed)
    {
        _mqttClient = client;
        _ledScreen = ledScreen;
        buttonOrange = orange;
        buttonBlue = blue;
        buttonRed = red;
        this.orangeLed = orangeLed;
        this.blueLed = blueLed;
        this.redLed = redLed;

    }
    private async Task ScrollActivitiesAsync()
{
    string line1 = "CHOOSE ACTIVITY";
    string line2 = "ORANGE:QUIZ   BLUE:SIMON SAYS   RED:REACTION GAME";

    int index = 0;

    while (state == InteractionState.ChoosingActivity)
    {
        string view = line2.Substring(index, 16);

        _ledScreen.SetTextNoRefresh($"{line1,-16}\n{view}");

        index++;
        if (index > line2.Length - 16)
            index = 0;

        await Task.Delay(300);
    }
}
    public void StartInteraction()
    {
        interactionStart = DateTime.Now;
        currentResult.StartTime = interactionStart;
        currentResult.Date = DateTime.Now.Date;
        state = InteractionState.ChoosingActivity;
        Robot.PlayNotes("L16EGC6G6");
        _ = ScrollActivitiesAsync();
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
        redLed.SetOn();
        orangeLed.SetOn();
        blueLed.SetOn();
        if ((DateTime.Now - lastButtonPress).TotalMilliseconds < DebounceMs)
        {
            return;
        }

        if (buttonBlue.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            redLed.SetOff();
            orangeLed.SetOff();
            PrepareGame("Simon Says", new SimonSays(buttonRed, buttonOrange, buttonBlue, _ledScreen, orangeLed, blueLed, redLed));
        }
        else if (buttonOrange.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            redLed.SetOff();
            blueLed.SetOff();
            PrepareGame("Quiz", new Quiz(_mqttClient, buttonRed, buttonOrange, buttonBlue, _ledScreen));
        }
        else if (buttonRed.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            orangeLed.SetOff();
            blueLed.SetOff();
            PrepareGame("Reaction Game", new ReactionGame(buttonRed, buttonOrange, buttonBlue, _ledScreen, orangeLed, blueLed, redLed));
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
                redLed.SetOff();
                orangeLed.SetOff();
                blueLed.SetOff();
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
