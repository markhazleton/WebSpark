namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a survey response answer.
/// </summary>
public partial class SurveyResponseAnswer
{
    /// <summary>
    /// Gets or sets the survey answer ID.
    /// </summary>
    [DisplayName("Survey Answer ID")]
    public int SurveyAnswerId { get; set; }

    /// <summary>
    /// Gets or sets the survey response ID.
    /// </summary>
    [DisplayName("Survey Response ID")]
    public int SurveyResponseId { get; set; }

    /// <summary>
    /// Gets or sets the sequence number.
    /// </summary>
    [DisplayName("Sequence Number")]
    public int SequenceNumber { get; set; }

    /// <summary>
    /// Gets or sets the question ID.
    /// </summary>
    [DisplayName("Question ID")]
    public int QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the question answer ID.
    /// </summary>
    [DisplayName("Question Answer ID")]
    public int QuestionAnswerId { get; set; }

    /// <summary>
    /// Gets or sets the answer type.
    /// </summary>
    [DisplayName("Answer Type")]
    public string AnswerType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the answer quantity.
    /// </summary>
    [DisplayName("Answer Quantity")]
    public double? AnswerQuantity { get; set; }

    /// <summary>
    /// Gets or sets the answer date.
    /// </summary>
    [DisplayName("Answer Date")]
    public DateTime? AnswerDate { get; set; }

    /// <summary>
    /// Gets or sets the answer comment.
    /// </summary>
    [DisplayName("Answer Comment")]
    public string? AnswerComment { get; set; }

    /// <summary>
    /// Gets or sets the modified comment.
    /// </summary>
    [DisplayName("Modified Comment")]
    public string? ModifiedComment { get; set; }

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the associated question.
    /// </summary>
    [DisplayName("Question")]
    public virtual Question Question { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associated question answer.
    /// </summary>
    [DisplayName("Question Answer")]
    public virtual QuestionAnswer QuestionAnswer { get; set; } = null!;

    public virtual SurveyResponseSequence SequenceNumberNavigation { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associated survey response sequence.
    /// </summary>
    [DisplayName("Survey Response Sequence")]
    public virtual SurveyResponseSequence SurveyResponse { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of survey response answer reviews.
    /// </summary>
    [DisplayName("Survey Response Answer Reviews")]
    public virtual ICollection<SurveyResponseAnswerReview> SurveyResponseAnswerReviews { get; set; } = [];
}
