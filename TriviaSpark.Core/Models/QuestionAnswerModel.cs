using System.ComponentModel.DataAnnotations;

namespace TriviaSpark.Core.Models;

public class QuestionAnswerModel
{
    public QuestionAnswerModel()
    {
        IsValid = false;
        ErrorMessage = "No Trivia QuestionId specified.";
        IsCorrect = false;
        AnswerText = string.Empty;
    }

    [Key]
    public int AnswerId { get; set; }
    public string QuestionId { get; set; }
    public string AnswerText { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsValid { get; set; }
    public int Value { get; set; }
    public string ErrorMessage { get; set; }
    public string ElapsedTime { get; set; }
}

