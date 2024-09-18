namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a question answer.
/// </summary>
public partial class QuestionAnswer
{
    /// <summary>
    /// Gets or sets the question answer ID.
    /// </summary>
    [Key]
    public int QuestionAnswerId { get; set; }

    /// <summary>
    /// Gets or sets the question ID. (foreign key)
    /// </summary>
    [ForeignKey("Question")]
    public int QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the question answer sort.
    /// </summary>
    [DisplayName("Sort")]
    public int QuestionAnswerSort { get; set; }

    /// <summary>
    /// Gets or sets the short name of the question answer.
    /// </summary>
    [DisplayName("Short CompanyNm")]
    public string QuestionAnswerShortNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the question answer.
    /// </summary>
    [DisplayName("CompanyNm")]
    public string QuestionAnswerNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the value of the question answer.
    /// </summary>
    [DisplayName("Value or Score")]
    public int QuestionAnswerValue { get; set; }

    /// <summary>
    /// Gets or sets the description of the question answer.
    /// </summary>
    [DisplayName("Description")]
    public string QuestionAnswerDs { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the question answer has comments.
    /// </summary>
    [DisplayName("Has Comments")]
    public bool CommentFl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the question answer is active.
    /// </summary>
    [DisplayName("Is Active")]
    public bool ActiveFl { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified the question answer.
    /// </summary>
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the question answer was last modified.
    /// </summary>
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the associated question.
    /// </summary>
    [ForeignKey("QuestionId")]
    public virtual Question Question { get; set; } = null!;

    /// <summary>
    /// The collection of survey response answers associated with the question answer.
    /// </summary>
    [ForeignKey("QuestionAnswerId")]
    public virtual ICollection<SurveyResponseAnswer> SurveyResponseAnswers { get; set; } = new List<SurveyResponseAnswer>();
}
