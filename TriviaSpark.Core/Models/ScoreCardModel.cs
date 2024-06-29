namespace TriviaSpark.Core.Models;

/// <summary>
/// Represents a scorecard for a quiz or trivia game.
/// </summary>
public class ScoreCardModel
{
    /// <summary>
    /// The adjusted score for the scorecard.
    /// </summary>
    public double AdjustedScore { get; set; }

    /// <summary>
    /// The number of correct answers for the scorecard.
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// The number of incorrect answers for the scorecard.
    /// </summary>
    public int IncorrectAnswers { get; set; }

    /// <summary>
    /// The total number of questions for the scorecard.
    /// </summary>
    public int QuestionCount { get; set; }
    /// <summary>
    /// The percentage of correct answers for the scorecard.
    /// </summary>
    public double PercentCorrect { get; set; }
    /// <summary>
    /// The Nubmer of Questions Attempted
    /// </summary>
    public int QuestionsAttempted { get; set; }
    public double PercentComplete
    {
        get
        {
            return Math.Round(QuestionsAttempted / (double)QuestionCount, 2);
        }
    }
}

