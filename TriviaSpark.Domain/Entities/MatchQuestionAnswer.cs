namespace TriviaSpark.Domain.Entities;

public class MatchQuestionAnswer : BaseEntity
{
    public MatchQuestionAnswer()
    {

    }
    public string QuestionId { get; set; }
    public int AnswerId { get; set; }
    public int MatchId { get; set; }
    public virtual Match Match { get; set; }
    public virtual Question Question { get; set; }
    public virtual QuestionAnswer Answer { get; set; }
    public string? Comment { get; set; }
    public int ElapsedTime { get; set; }
}

