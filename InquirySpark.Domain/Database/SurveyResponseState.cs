namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents the state of a survey response.
/// </summary>
public partial class SurveyResponseState
{
    /// <summary>
    /// Gets or sets the ID of the survey response state.
    /// </summary>
    [DisplayName("Survey Response State ID")]
    public int SurveyResponseStateId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the survey response.
    /// </summary>
    [DisplayName("Survey Response ID")]
    public int SurveyResponseId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the status.
    /// </summary>
    [DisplayName("Status ID")]
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the assigned user.
    /// </summary>
    [DisplayName("Assigned User ID")]
    public int AssignedUserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the survey response state is active.
    /// </summary>
    [DisplayName("Active")]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an email has been sent for the survey response state.
    /// </summary>
    [DisplayName("Email Sent")]
    public bool EmailSent { get; set; }

    /// <summary>
    /// Gets or sets the email body for the survey response state.
    /// </summary>
    [DisplayName("Email Body")]
    public string? EmailBody { get; set; }

    /// <summary>
    /// Gets or sets the ID of the modifier.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the survey response state was last modified.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the assigned user for the survey response state.
    /// </summary>
    [DisplayName("Assigned User")]
    public virtual ApplicationUser AssignedUser { get; set; } = null!;

    /// <summary>
    /// Gets or sets the status for the survey response state.
    /// </summary>
    [DisplayName("Status")]
    public virtual LuSurveyResponseStatus Status { get; set; } = null!;

    /// <summary>
    /// Gets or sets the survey response for the survey response state.
    /// </summary>
    [DisplayName("Survey Response")]
    public virtual SurveyResponse SurveyResponse { get; set; } = null!;
}
