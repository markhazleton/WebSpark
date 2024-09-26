namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a question type in the database.
/// </summary>
public partial class LuQuestionType
{
    /// <summary>
    /// Gets or sets the question type ID.
    /// </summary>
    [DisplayName("Question Type ID")]
    public int QuestionTypeId { get; set; }

    /// <summary>
    /// Gets or sets the question type code.
    /// </summary>
    [DisplayName("Question Type Code")]
    public string QuestionTypeCd { get; set; } = null!;

    /// <summary>
    /// Gets or sets the question type description.
    /// </summary>
    [DisplayName("Question Type Description")]
    public string QuestionTypeDs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the control name.
    /// </summary>
    [DisplayName("Control CompanyNm")]
    public string ControlName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the answer data type.
    /// </summary>
    [DisplayName("Answer Data Type")]
    public string AnswerDataType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date and time.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the collection of questions associated with this question type.
    /// </summary>
    [DisplayName("Questions")]
    public virtual ICollection<Question> Questions { get; set; } = [];
}
