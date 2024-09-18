namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents an error associated with a survey response answer.
/// </summary>
public partial class SurveyResponseAnswerError
{
    /// <summary>
    /// Gets or sets the unique identifier for the survey answer error.
    /// </summary>
    [DisplayName("Survey Answer Error ID")]
    public int SurveyAnswerErrorId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the survey response.
    /// </summary>
    [DisplayName("Survey Response ID")]
    public int SurveyResponseId { get; set; }

    /// <summary>
    /// Gets or sets the sequence number of the survey response answer.
    /// </summary>
    [DisplayName("Sequence Number")]
    public int SequenceNumber { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the question.
    /// </summary>
    [DisplayName("Question ID")]
    public int QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the question answer.
    /// </summary>
    [DisplayName("Question Answer ID")]
    public int? QuestionAnswerId { get; set; }

    /// <summary>
    /// Gets or sets the type of the answer.
    /// </summary>
    [DisplayName("Answer Type")]
    public string? AnswerType { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the answer.
    /// </summary>
    [DisplayName("Answer Quantity")]
    public string? AnswerQuantity { get; set; }

    /// <summary>
    /// Gets or sets the date of the answer.
    /// </summary>
    [DisplayName("Answer Date")]
    public string? AnswerDate { get; set; }

    /// <summary>
    /// Gets or sets the comment for the answer.
    /// </summary>
    [DisplayName("Answer Comment")]
    public string? AnswerComment { get; set; }

    /// <summary>
    /// Gets or sets the error code associated with the answer.
    /// </summary>
    [DisplayName("Error Code")]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error message associated with the answer.
    /// </summary>
    [DisplayName("Error Message")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the name of the program.
    /// </summary>
    [DisplayName("Program CompanyNm")]
    public string ProgramName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique identifier of the user who modified the answer.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the answer was modified.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }
}
