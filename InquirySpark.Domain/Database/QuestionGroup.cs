namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a question group.
/// </summary>
public partial class QuestionGroup
{
    /// <summary>
    /// Gets or sets the question group ID.
    /// </summary>
    [DisplayName("ID")]
    [Key]
    public int QuestionGroupId { get; set; }

    /// <summary>
    /// Gets or sets the survey for this group
    /// </summary>
    [DisplayName("Survey")]
    [ForeignKey("Survey")]
    public int SurveyId { get; set; }

    /// <summary>
    /// Gets or sets the group order.
    /// </summary>
    [DisplayName("Group Order")]
    public int GroupOrder { get; set; }

    /// <summary>
    /// Gets or sets the short name of the question group.
    /// </summary>
    [DisplayName("Group Short CompanyNm")]
    public string QuestionGroupShortNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the question group.
    /// </summary>
    [DisplayName("Group CompanyNm")]
    public string QuestionGroupNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the question group.
    /// </summary>
    [DisplayName("Group Description")]
    public string? QuestionGroupDs { get; set; }

    /// <summary>
    /// Gets or sets the weight of the question group.
    /// </summary>
    [DisplayName("Group Weight")]
    public decimal QuestionGroupWeight { get; set; }

    /// <summary>
    /// Gets or sets the header of the question group.
    /// </summary>
    [DisplayName("Group Header")]
    public string? GroupHeader { get; set; }

    /// <summary>
    /// Gets or sets the footer of the question group.
    /// </summary>
    [DisplayName("Group Footer")]
    public string? GroupFooter { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified the question group.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the question group was last modified.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the dependent question group.
    /// </summary>
    [DisplayName("Dependent Question Group ID")]
    public int? DependentQuestionGroupId { get; set; }

    /// <summary>
    /// Gets or sets the minimum score for the dependent question group.
    /// </summary>
    [DisplayName("Dependent Minimum Score")]
    public decimal? DependentMinScore { get; set; }

    /// <summary>
    /// Gets or sets the maximum score for the dependent question group.
    /// </summary>
    [DisplayName("Dependent Maximum Score")]
    public decimal? DependentMaxScore { get; set; }

    /// <summary>
    /// Gets or sets the question group members.
    /// </summary>
    [DisplayName("Members")]
    public virtual ICollection<QuestionGroupMember> QuestionGroupMembers { get; set; } = [];

    /// <summary>
    /// Gets or sets the survey associated with the question group.
    /// </summary>
    [DisplayName("Survey")]
    [ForeignKey("SurveyId")]
    public virtual Survey Survey { get; set; } = null!;
}
