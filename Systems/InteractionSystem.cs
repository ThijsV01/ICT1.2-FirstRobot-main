using System.Text.Json;
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
    private readonly Led RedLed = new Led(5);//aanpassen naar juiste nummer
    private readonly Button buttonRed = new Button(6);//aanpassen naar juiste nummer
    private readonly Buzzer buzzer = new Buzzer(12, 100);
    private InteractionState state = InteractionState.None;
    private DateTime interactionStart;
    public bool IsActive => state != InteractionState.None;
    private IInteractionGame? currentGame;
    private GameResult? currentResult;

    private const int InteractionTimeoutMinutes = 5;
    public InteractionSystem(SimpleMqttClient client)
    {
        _mqttClient = client;
    }
    public void StartInteraction()
    {
        interactionStart = DateTime.Now;
        currentResult = new GameResult
        {
            StartTime = interactionStart
        };
        state = InteractionState.ChoosingActivity;
        Robot.PlayNotes("L16EGC6G6");
        ledScreen.SetText("CHOOSE \nACTIVITY");
    }
    public void EndInteraction()
    {
        state = InteractionState.None;
        SetResultValues();
        currentGame = null;
        _mqttClient.PublishMessage(JsonSerializer.Serialize(currentResult), "robot/2242722/interaction/eind");
        currentResult=null;
        Robot.PlayNotes("L16EGC6G6");
        ledScreen.SetText("");
    }
    private void HandleChoosing()
    {
        if (buttonBlue.GetState() == "Pressed")
        {
            ledScreen.SetText("Simon Says");
            currentResult!.KindOfGame="Simon Says";
            currentGame = new SimonSays();
            currentGame.StartGame();

            state = InteractionState.Playing;
        }
        else if (buttonOrange.GetState() == "Pressed")
        {
            ledScreen.SetText("Quiz");
            currentResult!.KindOfGame="Quiz";
            currentGame = new Quiz();
            currentGame.StartGame();

            state = InteractionState.Playing;
        }
        else if (buttonRed.GetState() == "Pressed")
        {
            ledScreen.SetText("ReactionGame");
            currentResult!.KindOfGame="ReactionGame";
            currentGame = new ReactionGame();
            currentGame.StartGame();

            state = InteractionState.Playing;
        }
    }
    private void SetResultValues()
    {
        
        currentResult!.EndTime=DateTime.Now;
        if (currentGame!.Result.InteractionState != null)
        {
            currentResult.InteractionState=currentGame!.Result.InteractionState;
        }
        else
        {
            currentResult.InteractionState="Not started";
        }
        currentResult.AverageReactionTimeMs=currentGame.Result.AverageReactionTimeMs;
        currentResult.CorrectlyAnsweredPercentage=currentGame.Result.CorrectlyAnsweredPercentage;
    }
    public void Update()
    {
        if (state == InteractionState.None)
        {
            return;
        }
        if ((DateTime.Now - interactionStart).TotalMinutes > InteractionTimeoutMinutes && state == InteractionState.ChoosingActivity)
        {
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
                if (currentGame!.IsFinished)
                {
                    EndInteraction();
                }
                currentGame.Update();
                break;
        }
    }
}
