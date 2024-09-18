namespace InquirySpark.Domain.Database;
/// <summary>
/// survey response answer review
/// </summary>
public partial class SurveyResponseAnswerReview
{
    /// <summary>
    /// Gets or sets the ID of the survey response answer review.
    /// </summary>
    [DisplayName("Survey Response Answer Review ID")]
    public int SurveyResponseAnswerReviewId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the survey answer.
    /// </summary>
    [DisplayName("Survey Answer ID")]
    public int SurveyAnswerId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the application user role.
    /// </summary>
    [DisplayName("Application User Role ID")]
    public int ApplicationUserRoleId { get; set; }

    /// <summary>
    /// Gets or sets the review level.
    /// </summary>
    [DisplayName("Review Level")]
    public int ReviewLevel { get; set; }

    /// <summary>
    /// Gets or sets the ID of the review status.
    /// </summary>
    [DisplayName("Review Status ID")]
    public int ReviewStatusId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the modifier.
    /// </summary>
    [DisplayName("Modifier ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date and time.
    /// </summary>
    [DisplayName("Modified Date and Time")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the modified comment.
    /// </summary>
    [DisplayName("Modified Comment")]
    public string ModifiedComment { get; set; } = null!;

    /// <summary>
    /// Gets or sets the application user role.
    /// </summary>
    public virtual ApplicationUserRole ApplicationUserRole { get; set; } = null!;

    /// <summary>
    /// Gets or sets the survey response answer.
    /// </summary>
    public virtual SurveyResponseAnswer SurveyAnswer { get; set; } = null!;
}
