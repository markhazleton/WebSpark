namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents an application survey.
/// </summary>
public partial class ApplicationSurvey
{
    /// <summary>
    /// Gets or sets the ID of the application survey.
    /// </summary>
    [DisplayName("Application Survey ID")]
    [Key]
    public int ApplicationSurveyId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the application.
    /// </summary>
    [DisplayName("Application")]
    public int ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the survey.
    /// </summary>
    [DisplayName("Survey")]
    public int SurveyId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the default role.
    /// </summary>
    [DisplayName("Default Role")]
    public int DefaultRoleId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the modifier.
    /// </summary>
    [DisplayName("Modifier ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date and time.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the associated application.
    /// </summary>
    [DisplayName("Application")]
    public virtual Application Application { get; set; } = null!;

    /// <summary>
    /// Gets or sets the default role.
    /// </summary>
    [DisplayName("Default Role")]
    public virtual Role DefaultRole { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associated survey.
    /// </summary>
    [DisplayName("Survey")]
    public virtual Survey Survey { get; set; } = null!;
}
