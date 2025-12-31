using System.Text.Json;
using SimpleMqtt;
using Avans.StatisticalRobot;

public class Quiz : IInteractionGame
{
    private readonly SimpleMqttClient _client;
    private static bool subscribed = false;
    private readonly Random _rnd = new Random();
    private bool finished = false;
    public bool IsFinished => finished;
    private readonly Button buttonRed;
    private readonly Button buttonOrange;
    private readonly Button buttonBlue;
    private readonly LCD16x2 lcd;
    private DateTime startTime;
    private TimeSpan TimeToPlay = TimeSpan.FromMinutes(5);
    private TimeSpan QuestionTimeLimit = TimeSpan.FromMinutes(10);
    private int totalQuestions;
    private int correctAnswered;
    private int currentQuestionIndex;
    private int currentCorrectAnswerIndex;
    private DateTime startResponseTime;
    private DateTime lastButtonPress = DateTime.MinValue;
    private const int DebounceMs = 250;
    private List<TimeSpan> ReactionTimes = [];
    private GameState state;
    public GameResult Result { get; private set; } = new();
    private enum GameState
    {
        WaitingForQuestions, AskingQuestion, WaitingForResponse, Finished
    }
    private List<QuestionAnswers> questionsAnswers = new();

    private class QuestionAnswers
    {
        public string? Question { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? WrongAnswer { get; set; }
        public string? WrongAnswer2 { get; set; }
    }

    public Quiz(SimpleMqttClient client, Button buttonRed, Button buttonOrange, Button buttonBlue, LCD16x2 lcd)
    {
        this.buttonRed = buttonRed;
        this.buttonOrange = buttonOrange;
        this.buttonBlue = buttonBlue;
        this.lcd = lcd;
        _client = client;
        if (!subscribed)
        {
            _client.OnMessageReceived += HandleIncomingQuestions;
            _ = _client.SubscribeToTopic("robot/2242722/activities/quizquestions");
            subscribed = true;
        }
    }

    private void HandleIncomingQuestions(object? sender, SimpleMqttMessage args)
    {
        if (state != GameState.WaitingForQuestions)
        {
            return;
        }
        if (args.Topic == "robot/2242722/activities/quizquestions" && !string.IsNullOrEmpty(args.Message))
        {
            questionsAnswers = JsonSerializer.Deserialize<List<QuestionAnswers>>(args.Message) ?? new List<QuestionAnswers>();

            if (questionsAnswers.Count > 0 && state == GameState.WaitingForQuestions)
            {
                currentQuestionIndex = 0;
                state = GameState.AskingQuestion;
                startTime = DateTime.Now;
                totalQuestions = questionsAnswers.Count;
            }
        }
    }

    public void StartGame()
    {
        _ = _client.PublishMessage($"{1}", "robot/2242722/activities/quizstarted");

        Result = new GameResult();
        finished = false;

        correctAnswered = 0;
        currentQuestionIndex = 0;
        ReactionTimes = [];
        state = GameState.WaitingForQuestions;
    }
    public void Update()
    {
        if (finished)
        {
            return;
        }

        if (state != GameState.WaitingForQuestions && DateTime.Now - startTime > TimeToPlay)
        {
            state = GameState.Finished;
        }
        switch (state)
        {
            case GameState.WaitingForQuestions:
                return;
            case GameState.AskingQuestion:
                HandleQuestion();
                break;
            case GameState.WaitingForResponse:
                HandleResponse();
                break;
            case GameState.Finished:
                if (currentQuestionIndex < totalQuestions)
                {
                    HandleFinished("Not Finished");
                    break;
                }
                else
                {
                    if (!finished)
                    {
                        HandleFinished("Finished");
                    }

                    break;
                }
        }
    }
    private void HandleQuestion()
    {
        if (currentQuestionIndex >= totalQuestions)
        {
            state = GameState.Finished;
            return;
        }
        var q = questionsAnswers[currentQuestionIndex];

        var answers = new List<string> { q.CorrectAnswer!, q.WrongAnswer!, q.WrongAnswer2! };
        answers = answers.OrderBy(_ => _rnd.Next()).ToList();

        //hier past niet alles in, dus daar moet ik nog naar kijken
        string displayText = $"{q.Question}\nR:{answers[0]} O:{answers[1]} B:{answers[2]}";
        lcd.SetText(displayText.Length <= 32 ? displayText : displayText.Substring(0, 32));

        currentCorrectAnswerIndex = answers.IndexOf(q.CorrectAnswer!);
        startResponseTime = DateTime.Now;
        state = GameState.WaitingForResponse;
    }
    private void HandleResponse()
    {
        if ((DateTime.Now - lastButtonPress).TotalMilliseconds < DebounceMs)
        {
            return;
        }

        if (DateTime.Now - startResponseTime > QuestionTimeLimit)
        {
            ReactionTimes.Add(QuestionTimeLimit);
            currentQuestionIndex++;
            if (currentQuestionIndex < totalQuestions)
            {
                state = GameState.AskingQuestion;
            }
            else
            {
                state = GameState.Finished;
            }
            return;
        }
        int? selected = null;
        if (buttonRed.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            ReactionTimes.Add(DateTime.Now - startResponseTime);
            selected = 0;
        }
        else if (buttonOrange.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            ReactionTimes.Add(DateTime.Now - startResponseTime);
            selected = 1;
        }
        else if (buttonBlue.GetState() == "Pressed")
        {
            lastButtonPress = DateTime.Now;
            ReactionTimes.Add(DateTime.Now - startResponseTime);
            selected = 2;
        }
        if (!selected.HasValue)
        {
            return;
        }

        if (selected.Value == currentCorrectAnswerIndex)
        {
            correctAnswered++;
        }

        currentQuestionIndex++;
        if (currentQuestionIndex < questionsAnswers.Count)
        {
            state = GameState.AskingQuestion;
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

        Result.CorrectlyAnsweredPercentage = (int)((double)correctAnswered / totalQuestions * 100);
        Result.InteractionState = state;
        Result.SimonSaysAmount = null;

        finished = true;
        subscribed = false;
        lcd.SetText("QUIZ\nFINISHED");
    }
}