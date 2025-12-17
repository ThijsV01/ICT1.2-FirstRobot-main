public class Quiz : IInteractionGame
{
    private bool finished = false;
    public bool IsFinished => finished;
    private DateTime startTime;
    private TimeSpan TimeToPlay = TimeSpan.FromMinutes(5);
    private readonly int totalQuestions;
    private int questionsAnswered;
    private int currentQuestion;
    private DateTime startResponseTime;
    private GameState state;
    public GameResult Result { get; private set; } = new();
    private enum GameState
    {
        AskingQuestion, WaitingForResponse, Finished
    }
    public Quiz(int totalQuestions=5)
    {
        this.totalQuestions=totalQuestions;
    }

    public void StartGame()
    {
        Result=new GameResult();
        questionsAnswered = 0;
        startTime = DateTime.Now;
        finished = false;
        state = GameState.AskingQuestion;
        HandleQuestion();
    }
    public void Update()
    {
        switch (state)
        {
            case GameState.AskingQuestion:
                HandleQuestion();
                break;
            case GameState.WaitingForResponse:
                HandleResponse();
                break;
            case GameState.Finished:
                if (questionsAnswered < totalQuestions)
                {
                    HandleFinished("Not Finished");
                    return;
                }
                else
                {
                    HandleFinished("Finished");
                    return;
                }
        }
        if (DateTime.Now - startTime > TimeToPlay)
        {
            state = GameState.Finished;
        }
    }
    private void HandleQuestion()
    {
        //vraag stellen
        state = GameState.WaitingForResponse;
        startResponseTime=DateTime.Now;
        currentQuestion++;
    }
    private void HandleResponse()
    {
        if (DateTime.Now - startResponseTime > TimeToPlay)
        {
            state = GameState.Finished;
            return;
        }
        if (questionsAnswered+1 > totalQuestions)
        {
            state = GameState.Finished;
            return;
        }
        //hier antwoord verwerken
        //bij antwoord gegeven pas questionsAnswered++;
    }
    private void HandleFinished(string state)
    {
        Result.AverageReactionTimeMs=5;//een variabele die ik vul
        Result.CorrectlyAnsweredPercentage=5;//een variabele die ik vul
        Result.InteractionState=state;
        finished = true;
    }
}