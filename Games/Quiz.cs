public class Quiz:IInteractionGame
{
    private bool finished=false;
    public bool IsFinished => finished;

    public void StartGame()
    {
        finished=false;
    }
    public void Update()
    {
        //hier de quiz en finished naar false wanneer alle vragen voorbij zijn is of wanneer tijd voorbij is ofzo.
    }
}