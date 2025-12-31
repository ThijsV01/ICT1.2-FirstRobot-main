public class GameResult
{
    public int RobotId { get; set; }
    public DateTime Date{get;set;}
    public DateTime StartTime{get;set;}
    public DateTime EndTime{get;set;}
    public string? KindOfGame{get;set;}
    public int AverageReactionTimeMs{get;set;}
    public int? CorrectlyAnsweredPercentage{get;set;}
    public string? InteractionState{get;set;}
    public int? SimonSaysAmount{ get; set; }
}