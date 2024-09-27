namespace InquirySpark.Domain.Database;

/// <summary>
/// survey response sequence.
/// </summary>
public partial class SurveyResponseSequence
{
    /// <summary>
    /// Gets or sets the survey response sequence ID.
    /// </summary>
    [DisplayName("Survey Response Sequence ID")]
    public int SurveyResponseSequenceId { get; set; }

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
    /// Gets or sets the sequence text.
    /// </summary>
    [DisplayName("Sequence Text")]
    public string? SequenceText { get; set; }

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date and time.
    /// </summary>
    [DisplayName("Modified Date and Time")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the survey response.
    /// </summary>
    [DisplayName("Survey Response")]
    public virtual SurveyResponse SurveyResponse { get; set; } = null!;

    public virtual ICollection<SurveyResponseAnswer> SurveyResponseAnswerSequenceNumberNavigations { get; set; } = [];

    /// <summary>
    /// Gets or sets the survey response answers.
    /// </summary>
    [DisplayName("Survey Response Answers")]
    public virtual ICollection<SurveyResponseAnswer> SurveyResponseAnswerSurveyResponses { get; set; } = [];
}
