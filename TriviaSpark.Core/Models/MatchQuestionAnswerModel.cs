namespace TriviaSpark.Core.Models;

/// <summary>
/// Model representing the match between a question and its answer.
/// </summary>
public class MatchQuestionAnswerModel
{
    /// <summary>
    /// Gets or sets the identifier of the question.
    /// </summary>
    public string QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the answer.
    /// </summary>
    public int AnswerId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the match.
    /// </summary>
    public int MatchId { get; set; }
}

