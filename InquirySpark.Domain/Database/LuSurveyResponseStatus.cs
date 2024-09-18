namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents the status of a survey response.
/// </summary>
[DisplayName("LuSurveyResponseStatus")]
public partial class LuSurveyResponseStatus
{
    /// <summary>
    /// Gets or sets the status ID.
    /// </summary>
    [DisplayName("Status ID")]
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the status name.
    /// </summary>
    [DisplayName("Status CompanyNm")]
    public string StatusNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the status description.
    /// </summary>
    [DisplayName("Status Description")]
    public string StatusDs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email template.
    /// </summary>
    [DisplayName("Email Template")]
    public string? EmailTemplate { get; set; }

    /// <summary>
    /// Gets or sets the previous status ID.
    /// </summary>
    [DisplayName("Previous Status ID")]
    public int PreviousStatusId { get; set; }

    /// <summary>
    /// Gets or sets the next status ID.
    /// </summary>
    [DisplayName("Next Status ID")]
    public int NextStatusId { get; set; }

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date and time.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the survey response states associated with this status.
    /// </summary>
    [DisplayName("Survey Response States")]
    public virtual ICollection<SurveyResponseState> SurveyResponseStates { get; set; } = new List<SurveyResponseState>();
}
