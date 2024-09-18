using System.ComponentModel.DataAnnotations;

namespace TriviaSpark.Domain.Entities;

public class QuestionAnswer : BaseEntity
{
    public QuestionAnswer()
    {
        IsValid = false;
        ErrorMessage = "No Trivia QuestionId specified.";
        IsCorrect = false;
        AnswerText = string.Empty;
    }

    [Key]
    public int AnswerId { get; set; }
    public string QuestionId { get; set; }
    public Question Question { get; set; }
    public string AnswerText { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsValid { get; set; }
    public int Value { get; set; }
    public string ErrorMessage { get; set; }
    public virtual ICollection<MatchQuestionAnswer> MatchQuestionAnswers { get; set; }
}

