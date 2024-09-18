namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents the status of a survey review.
/// </summary>
public partial class SurveyReviewStatus
{
    /// <summary>
    /// Gets or sets the ID of the survey review status.
    /// </summary>
    [DisplayName("Survey Review Status ID")]
    public int SurveyReviewStatusId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the survey.
    /// </summary>
    [DisplayName("Survey ID")]
    public int SurveyId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the review status.
    /// </summary>
    [DisplayName("Review Status ID")]
    public int ReviewStatusId { get; set; }

    /// <summary>
    /// Gets or sets the name of the review status.
    /// </summary>
    [DisplayName("Review Status CompanyNm")]
    public string ReviewStatusNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the review status.
    /// </summary>
    [DisplayName("Review Status Description")]
    public string ReviewStatusDs { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the survey review is approved.
    /// </summary>
    [DisplayName("Approved Flag")]
    public bool ApprovedFl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the survey review has comments.
    /// </summary>
    [DisplayName("Comment Flag")]
    public bool CommentFl { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified the survey review status.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the survey review status was last modified.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the survey associated with the survey review status.
    /// </summary>
    [DisplayName("Survey")]
    public virtual Survey Survey { get; set; } = null!;
}
