namespace InquirySpark.Domain.Database;

/// <summary>
/// survey response history
/// </summary>
public partial class SurveyResponseHistory
{
    /// <summary>
    /// Gets or sets the ID of the survey response history.
    /// </summary>
    public int SurveyResponseHistoryId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the application user.
    /// </summary>
    public int ApplicationUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the survey response.
    /// </summary>
    public int SurveyResponseId { get; set; }

    /// <summary>
    /// Gets or sets the name of the survey response.
    /// </summary>
    [DisplayName("Survey Response CompanyNm")]
    public string SurveyResponseNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ID of the status.
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the question group.
    /// </summary>
    public int? QuestionGroupId { get; set; }

    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    [DisplayName("User CompanyNm")]
    public string? UserNm { get; set; }

    /// <summary>
    /// Gets or sets the answers.
    /// </summary>
    public string? Answers { get; set; }

    /// <summary>
    /// Gets or sets the ID of the modifier.
    /// </summary>
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the survey response.
    /// </summary>
    public virtual SurveyResponse SurveyResponse { get; set; } = null!;
}
