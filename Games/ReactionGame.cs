using Avans.StatisticalRobot;
using SimpleMqtt;

public class ReactionGame : IInteractionGame
{
    private bool finished = false;
    public bool IsFinished => finished;
    private readonly Button buttonRed;
    private readonly Button buttonOrange;
    private readonly Button buttonBlue;
    private readonly Led orangeLed;
    private readonly Led blueLed;
    private readonly Led redLed;
    private readonly LCD16x2 lcd;
    private int totalReactionTrainingMoments;
    private int maxReactionTrainingMoments = 5;
    private int correctAnswered;
    private Led? activeLed;
    private Button? correctButton;
    private List<TimeSpan> ReactionTimes = [];
    private DateTime startResponseTime;
    private DateTime lastButtonPress = DateTime.MinValue;
    private const int DebounceMs = 250;
    private static readonly Random rnd = new Random();
    public GameResult Result { get; private set; } = new();
    private GameState state;
    private enum GameState
    {
        WaitingForLightOn, ProcessingReactionTime, Finished
    }
    public ReactionGame(Button buttonRed, Button buttonOrange, Button buttonBlue, LCD16x2 lcd, Led orangeLed, Led blueLed, Led redLed)
    {
        this.buttonRed = buttonRed;
        this.buttonOrange = buttonOrange;
        this.buttonBlue = buttonBlue;
        this.blueLed = blueLed;
        this.orangeLed = orangeLed;
        this.redLed = redLed;
        this.lcd = lcd;
    }

    public void StartGame()
    {
        Result = new GameResult();
        finished = false;
        correctAnswered = 0;
        ReactionTimes = [];
        totalReactionTrainingMoments = 0;
        state = GameState.WaitingForLightOn;
    }
    public void Update()
    {
        switch (state)
        {
            case GameState.WaitingForLightOn:
                HandleWaiting();
                break;
            case GameState.ProcessingReactionTime:
                HandleResponse();
                break;
            case GameState.Finished:

                if (!finished)
                {
                    HandleFinished("Finished");
                }
                break;

        }
    }
    private void HandleWaiting()
    {
        int choice = rnd.Next(3);
        switch (choice)
        {
            case 0:
                activeLed = redLed;
                correctButton = buttonRed;
                break;
            case 1:
                activeLed = orangeLed;
                correctButton = buttonOrange;
                break;
            case 2:
                activeLed = blueLed;
                correctButton = buttonBlue;
                break;
        }
        redLed.SetOff();
        orangeLed.SetOff();
        blueLed.SetOff();

        lcd.SetText($"Press the lit\nbutton!");
        startResponseTime = DateTime.Now;
        activeLed!.SetOn();
        state = GameState.ProcessingReactionTime;
    }
    private void HandleResponse()
    {
        if ((DateTime.Now - lastButtonPress).TotalMilliseconds < DebounceMs)
        {
            return;
        }
    
        var pressedButton = GetPressedButton();
        if (pressedButton == null)
        {
            return;
        }
            

        var reactionTime = DateTime.Now - startResponseTime;
        ReactionTimes.Add(reactionTime);

        if (pressedButton == correctButton)
        {
            correctAnswered++;
        }
        totalReactionTrainingMoments++;

        redLed.SetOff();
        orangeLed.SetOff();
        blueLed.SetOff();
        if (totalReactionTrainingMoments < maxReactionTrainingMoments)
        {
            state = GameState.WaitingForLightOn;
        }
        else
        {
            state = GameState.Finished;
        }
    }
    private void HandleFinished(string state)
    {
        if (ReactionTimes.Count > 0)
        {
            double avg = ReactionTimes.Average(rt => rt.TotalMilliseconds);
            Result.AverageReactionTimeMs = (int)Math.Round(avg);
        }
        else
        {
            Result.AverageReactionTimeMs = -1;
        }

        Result.CorrectlyAnsweredPercentage = (int)((double)correctAnswered / maxReactionTrainingMoments * 100);
        Result.InteractionState = state;
        Result.SimonSaysAmount = null;
        finished = true;
        lcd.SetText("REACTION\nFINISHED");
    }
    private Button? GetPressedButton()
    {
        if (buttonRed.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            return buttonRed;
        }
        if (buttonOrange.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            return buttonOrange;
        }
        if (buttonBlue.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            return buttonBlue;
        }
        return null;
    }
}