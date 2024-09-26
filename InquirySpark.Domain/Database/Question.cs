namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a question in the database.
/// </summary>
public partial class Question
{
    /// <summary>
    /// Gets or sets the question ID.
    /// </summary>
    [DisplayName("ID")]
    public int QuestionId { get; set; }

    /// <summary>
    /// Gets or sets the survey type ID.
    /// </summary>
    [DisplayName("Survey Type")]
    public int SurveyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the short name of the question.
    /// </summary>
    [DisplayName("Short CompanyNm")]
    public string QuestionShortNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the question.
    /// </summary>
    [DisplayName("CompanyNm")]
    public string QuestionNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the question.
    /// </summary>
    [DisplayName("Description")]
    public string QuestionDs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the keywords associated with the question.
    /// </summary>
    [DisplayName("Keywords")]
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets the sort order of the question.
    /// </summary>
    [DisplayName("Sort Order")]
    public int QuestionSort { get; set; }

    /// <summary>
    /// Gets or sets the review role level of the question.
    /// </summary>
    [DisplayName("Review Role Level")]
    public int ReviewRoleLevel { get; set; }

    /// <summary>
    /// Gets or sets the question type ID.
    /// </summary>
    [DisplayName("Question Type")]
    public int QuestionTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether comments are allowed for the question.
    /// </summary>
    [DisplayName("Allow Comments")]
    public bool CommentFl { get; set; }

    /// <summary>
    /// Gets or sets the value of the question.
    /// </summary>
    [DisplayName("Value")]
    public int QuestionValue { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure ID.
    /// </summary>
    [DisplayName("Unit of Measure")]
    public int UnitOfMeasureId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified the question.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the question was last modified.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the file data associated with the question.
    /// </summary>
    [DisplayName("File Data")]
    public byte[]? FileData { get; set; }

    /// <summary>
    /// Gets or sets the collection of question answers.
    /// </summary>
    [DisplayName("Question Answers")]
    public virtual ICollection<QuestionAnswer> QuestionAnswers { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of question group members.
    /// </summary>
    [DisplayName("Question Group Members")]
    public virtual ICollection<QuestionGroupMember> QuestionGroupMembers { get; set; } = [];

    /// <summary>
    /// Gets or sets the question type.
    /// </summary>
    [DisplayName("Question Type")]
    public virtual LuQuestionType QuestionType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of survey response answers.
    /// </summary>
    [DisplayName("Survey Response Answers")]
    public virtual ICollection<SurveyResponseAnswer> SurveyResponseAnswers { get; set; } = [];

    /// <summary>
    /// Gets or sets the survey type.
    /// </summary>
    [DisplayName("Survey Type")]
    public virtual LuSurveyType SurveyType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unit of measure.
    /// </summary>
    [DisplayName("Unit of Measure")]
    public virtual LuUnitOfMeasure UnitOfMeasure { get; set; } = null!;
}
