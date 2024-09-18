namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a member of a question group.
/// </summary>
public partial class QuestionGroupMember
{
    /// <summary>
    /// Gets or sets the ID of the question group member.
    /// </summary>
    [DisplayName("Question Group Member ID")]
    public int QuestionGroupMemberId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the question group.
    /// </summary>
    [DisplayName("Question Group ID")]
    public int QuestionGroupId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the question.
    /// </summary>
    [DisplayName("Question ID")]
    public int QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the weight of the question.
    /// </summary>
    [DisplayName("Question Weight")]
    public decimal QuestionWeight { get; set; }

    /// <summary>
    /// Gets or sets the display order of the question.
    /// </summary>
    [DisplayName("Display Order")]
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the ID of the modifier.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the question group member was last modified.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the question associated with the question group member.
    /// </summary>
    [DisplayName("Question")]
    public virtual Question Question { get; set; } = null!;

    /// <summary>
    /// Gets or sets the question group associated with the question group member.
    /// </summary>
    [DisplayName("Question Group")]
    public virtual QuestionGroup QuestionGroup { get; set; } = null!;
}
