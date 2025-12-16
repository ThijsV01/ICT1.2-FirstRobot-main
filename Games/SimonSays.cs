public class SimonSays:IInteractionGame
{
    private bool finished=false;
    public bool IsFinished => finished;

    public void StartGame()
    {
        finished=false;
    }
    public void Update()
    {
        //hier de simon says en finished naar false wanneer gekozen ding fout is of wanneer tijd voorbij is ofzo.
    }
}