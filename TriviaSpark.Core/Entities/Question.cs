using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriviaSpark.Core.Entities;

public class Question : BaseEntity
{
    [Key]
    public string QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string Category { get; set; }
    public Models.Difficulty Difficulty { get; set; }
    public Models.QuestionType Type { get; set; }
    public string Source { get; set; }
    public virtual ICollection<MatchQuestion> MatchQuestions { get; set; }
    public virtual ICollection<MatchQuestionAnswer> MatchQuestionAnswers { get; set; }
    public virtual ICollection<QuestionAnswer> Answers { get; set; }

    [NotMapped]
    public string CorrectAnswer
    {
        get
        {
            Answers ??= [];

            return Answers.Where(w => w.IsCorrect == true).Select(s => s.AnswerText).FirstOrDefault();
        }
    }

    [NotMapped]
    public List<string> IncorrectAnswers
    {
        get
        {
            Answers ??= [];

            return Answers.Where(w => w.IsCorrect == false).Select(s => s.AnswerText).ToList();
        }
    }
}

