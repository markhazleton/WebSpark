namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a survey email template.
/// </summary>
public partial class SurveyEmailTemplate
{
    /// <summary>
    /// Gets or sets the survey email template ID.
    /// </summary>
    [DisplayName("Survey Email Template ID")]
    public int SurveyEmailTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the survey email template name.
    /// </summary>
    [DisplayName("Survey Email Template CompanyNm")]
    public string SurveyEmailTemplateNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the survey ID.
    /// </summary>
    [DisplayName("Survey ID")]
    public int SurveyId { get; set; }

    /// <summary>
    /// Gets or sets the status ID.
    /// </summary>
    [DisplayName("Status ID")]
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the subject template.
    /// </summary>
    [DisplayName("Subject Template")]
    public string SubjectTemplate { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email template.
    /// </summary>
    [DisplayName("Email Template")]
    public string EmailTemplate { get; set; } = null!;

    /// <summary>
    /// Gets or sets the from email address.
    /// </summary>
    [DisplayName("From Email Address")]
    public string FromEmailAddress { get; set; } = null!;

    /// <summary>
    /// Gets or sets the filter criteria.
    /// </summary>
    [DisplayName("Filter Criteria")]
    public string? FilterCriteria { get; set; }

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    [DisplayName("Start Date")]
    public DateTime? StartDt { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    [DisplayName("End Date")]
    public DateTime? EndDt { get; set; }

    /// <summary>
    /// Gets or sets the active status.
    /// </summary>
    [DisplayName("Active")]
    public bool? Active { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to send the email to supervisor.
    /// </summary>
    [DisplayName("Send To Supervisor")]
    public bool SendToSupervisor { get; set; }

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
    /// Gets or sets the associated survey.
    /// </summary>
    [DisplayName("Survey")]
    public virtual Survey Survey { get; set; } = null!;
}
