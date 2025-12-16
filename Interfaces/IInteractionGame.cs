public interface IInteractionGame
{
    void StartGame();
    void Update();
    bool IsFinished { get; }
}