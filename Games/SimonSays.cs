using Avans.StatisticalRobot;
using SimpleMqtt;

public class SimonSays : IInteractionGame
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
    private List<SimonStep> SimonSaysListDuringGame = [];
    private List<TimeSpan> ReactionTimes = [];
    private int currentInputIndex = 0;
    private DateTime inputStartTime;
    private DateTime lastButtonPress = DateTime.MinValue;
    private const int DebounceMs = 250;
    private int showIndex = 0;
    private bool ledOn = false;
    private DateTime nextStepTime = DateTime.MinValue;
    private static readonly Random rnd = new Random();
    public GameResult Result { get; private set; } = new();
    private GameState state;
    private enum GameState
    {
        ShowingCurrentSimonSaysList, ProcessingSimonSaysInteraction, UpdatingSimonSaysList, Finished
    }
    public SimonSays(Button buttonRed, Button buttonOrange, Button buttonBlue, LCD16x2 lcd, Led orangeLed, Led blueLed, Led redLed)
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
        SimonSaysListDuringGame = [];
        ReactionTimes = [];
        currentInputIndex = 0;
        state = GameState.UpdatingSimonSaysList;
    }
    public void Update()
    {
        switch (state)
        {
            case GameState.UpdatingSimonSaysList:
                ExpandSequence();
                break;
            case GameState.ShowingCurrentSimonSaysList:
                ShowRightSequence();
                break;
            case GameState.ProcessingSimonSaysInteraction:
                HandleInputSequence();
                break;
            case GameState.Finished:
                if (!finished)
                {
                    HandleFinished("Finished");
                }
                break;

        }
    }
    private void ExpandSequence()
    {
        currentInputIndex = 0;
        int choice = rnd.Next(3);
        switch (choice)
        {
            case 0:
                SimonSaysListDuringGame.Add(new SimonStep(buttonRed, redLed));
                break;
            case 1:
                SimonSaysListDuringGame.Add(new SimonStep(buttonOrange, orangeLed));
                break;
            case 2:
                SimonSaysListDuringGame.Add(new SimonStep(buttonBlue, blueLed));
                break;
        }
        state = GameState.ShowingCurrentSimonSaysList;
    }
    private void ShowRightSequence()
    {
        if (showIndex == 0 && !ledOn)
        {
            lcd.SetText("WATCH\nCLOSELY");
            nextStepTime = DateTime.Now;
        }

        if (DateTime.Now < nextStepTime)
            return;

        var step = SimonSaysListDuringGame[showIndex];

        if (!ledOn)
        {
            step.Led.SetOn();
            ledOn = true;
            nextStepTime = DateTime.Now.AddMilliseconds(500);
        }
        else
        {
            step.Led.SetOff();
            ledOn = false;
            showIndex++;
            nextStepTime = DateTime.Now.AddMilliseconds(300);

            if (showIndex >= SimonSaysListDuringGame.Count)
            {
                showIndex = 0;
                lcd.SetText("YOUR TURN:\nREPEAT IT");
                inputStartTime = DateTime.Now;
                currentInputIndex = 0;
                state = GameState.ProcessingSimonSaysInteraction;
            }
        }
    }
    private void HandleInputSequence()
    {
        if ((DateTime.Now - lastButtonPress).TotalMilliseconds < DebounceMs)
        {
            return;
        }
        Button? pressedButton = GetPressedButton();

        if (pressedButton == null)
        {
            return;
        }

        var reactionTime = DateTime.Now - inputStartTime;
        ReactionTimes.Add(reactionTime);

        if (pressedButton != SimonSaysListDuringGame[currentInputIndex].Button)
        {
            state = GameState.Finished;
            return;
        }

        currentInputIndex++;

        if (currentInputIndex >= SimonSaysListDuringGame.Count)
        {
            state = GameState.UpdatingSimonSaysList;
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

        Result.CorrectlyAnsweredPercentage = null;
        Result.InteractionState = state;
        Result.SimonSaysAmount = SimonSaysListDuringGame.Count;

        redLed.SetOff();
        orangeLed.SetOff();
        blueLed.SetOff();

        finished = true;
        lcd.SetText("SIMON SAYS\nFINISHED");
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
    public class SimonStep
    {
        public Button Button { get; }
        public Led Led { get; }

        public SimonStep(Button button, Led led)
        {
            Button = button;
            Led = led;
        }
    }
}