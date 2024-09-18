namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a survey type.
/// </summary>
[DisplayName("LuSurveyType")]
public partial class LuSurveyType
{
    /// <summary>
    /// Gets or sets the survey type ID.
    /// </summary>
    [DisplayName("Survey Type ID")]
    [Key]
    public int SurveyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the short name of the survey type.
    /// </summary>
    [DisplayName("Survey Type Short CompanyNm")]
    public string SurveyTypeShortNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the survey type.
    /// </summary>
    [DisplayName("Survey Type CompanyNm")]
    public string SurveyTypeNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the survey type.
    /// </summary>
    [DisplayName("Survey Type Description")]
    public string? SurveyTypeDs { get; set; }

    /// <summary>
    /// Gets or sets the comment for the survey type.
    /// </summary>
    [DisplayName("Survey Type Comment")]
    public string? SurveyTypeComment { get; set; }

    /// <summary>
    /// Gets or sets the application type ID.
    /// </summary>
    [DisplayName("Application Type ID")]
    public int ApplicationTypeId { get; set; }

    /// <summary>
    /// Gets or sets the parent survey type ID.
    /// </summary>
    [DisplayName("Parent Survey Type ID")]
    public int? ParentSurveyTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the survey type has multiple sequences.
    /// </summary>
    [DisplayName("Multiple Sequence Flag")]
    public bool MutiSequenceFl { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified the survey type.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the survey type was last modified.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the collection of questions associated with the survey type.
    /// </summary>
    [DisplayName("Questions")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    /// <summary>
    /// Gets or sets the collection of surveys associated with the survey type.
    /// </summary>
    [DisplayName("Surveys")]
    public virtual ICollection<Survey> Surveys { get; set; } = new List<Survey>();
}
