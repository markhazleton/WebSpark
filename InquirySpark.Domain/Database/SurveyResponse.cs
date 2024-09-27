namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a survey response.
/// </summary>
public partial class SurveyResponse
{
    /// <summary>
    /// Gets or sets the survey response ID.
    /// </summary>
    [DisplayName("Survey Response ID")]
    public int SurveyResponseId { get; set; }

    /// <summary>
    /// Gets or sets the survey response name.
    /// </summary>
    [DisplayName("Survey Response CompanyNm")]
    public string SurveyResponseNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the survey ID.
    /// </summary>
    [DisplayName("Survey ID")]
    public int SurveyId { get; set; }

    /// <summary>
    /// Gets or sets the application ID.
    /// </summary>
    [DisplayName("Application ID")]
    public int ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the assigned user ID.
    /// </summary>
    [DisplayName("Assigned User ID")]
    public int? AssignedUserId { get; set; }

    /// <summary>
    /// Gets or sets the status ID.
    /// </summary>
    [DisplayName("Status ID")]
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the data source.
    /// </summary>
    [DisplayName("Data Source")]
    public string DataSource { get; set; } = null!;

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
    /// Gets or sets the application associated with the survey response.
    /// </summary>
    [DisplayName("Application")]
    public virtual Application Application { get; set; } = null!;

    /// <summary>
    /// Gets or sets the assigned user for the survey response.
    /// </summary>
    [DisplayName("Assigned User")]
    public virtual ApplicationUser? AssignedUser { get; set; }

    /// <summary>
    /// Gets or sets the survey associated with the survey response.
    /// </summary>
    [DisplayName("Survey")]
    public virtual Survey Survey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of survey response histories.
    /// </summary>
    [DisplayName("Survey Response Histories")]
    public virtual ICollection<SurveyResponseHistory> SurveyResponseHistories { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of survey response sequences.
    /// </summary>
    [DisplayName("Survey Response Sequences")]
    public virtual ICollection<SurveyResponseSequence> SurveyResponseSequences { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of survey response states.
    /// </summary>
    [DisplayName("Survey Response States")]
    public virtual ICollection<SurveyResponseState> SurveyResponseStates { get; set; } = [];
}
