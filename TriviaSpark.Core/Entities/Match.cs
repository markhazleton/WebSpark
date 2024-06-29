using System.ComponentModel.DataAnnotations;

namespace TriviaSpark.Core.Entities;

public class Match : BaseEntity
{
    [Key]
    public int MatchId { get; set; }
    public string UserId { get; set; }
    public string MatchName { get; set; }
    public Models.MatchMode MatchMode { get; set; }
    public Models.Difficulty Difficulty { get; set; }
    public Models.QuestionType QuestionType { get; set; }

    public virtual ICollection<MatchQuestion> MatchQuestions { get; set; }
    public virtual ICollection<MatchQuestionAnswer> MatchQuestionAnswers { get; set; }

    // Foreign key to User table
    public virtual TriviaSparkWebUser User { get; set; }
}

