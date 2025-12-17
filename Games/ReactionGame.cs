public class ReactionGame:IInteractionGame
{
    private bool finished=false;
    public bool IsFinished => finished;
    public GameResult Result { get; private set; } = new();

    public void StartGame()
    {
        finished=false;
    }
    public void Update()
    {
        //hier de reaction game en finished naar true wanneer aantal pogingen voorbij is of wanneer tijd voorbij is ofzo.
    }
}