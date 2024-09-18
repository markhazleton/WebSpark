namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents the review status of a LuReviewStatus.
/// </summary>
[DisplayName("LuReviewStatus")]
public partial class LuReviewStatus
{
    /// <summary>
    /// Gets or sets the review status ID.
    /// </summary>
    [DisplayName("Review Status ID")]
    public int ReviewStatusId { get; set; }

    /// <summary>
    /// Gets or sets the review status name.
    /// </summary>
    [DisplayName("Review Status CompanyNm")]
    public string ReviewStatusNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the review status description.
    /// </summary>
    [DisplayName("Review Status Description")]
    public string ReviewStatusDs { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the review is approved.
    /// </summary>
    [DisplayName("Approved Flag")]
    public bool ApprovedFl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the review has comments.
    /// </summary>
    [DisplayName("Comment Flag")]
    public bool CommentFl { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified the review status.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the review status was last modified.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }
}
