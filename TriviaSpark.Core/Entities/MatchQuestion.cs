namespace TriviaSpark.Core.Entities;

public class MatchQuestion : BaseEntity
{
    public string QuestionId { get; set; }
    public int MatchId { get; set; }
    public virtual Question Question { get; set; }
    public virtual Match Match { get; set; }

}

