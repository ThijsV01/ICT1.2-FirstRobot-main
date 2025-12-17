public class GameResult
{
    public DateTime StartTime{get;set;}
    public DateTime EndTime{get;set;}
    public string? KindOfGame{get;set;}
    public int AverageReactionTimeMs{get;set;}
    public int CorrectlyAnsweredPercentage{get;set;}
    public string? InteractionState{get;set;}
}