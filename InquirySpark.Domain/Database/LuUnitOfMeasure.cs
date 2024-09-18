namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a unit of measure.
/// </summary>
public partial class LuUnitOfMeasure
{
    /// <summary>
    /// Gets or sets the unit of measure ID.
    /// </summary>
    [DisplayName("Unit of Measure ID")]
    public int UnitOfMeasureId { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure name.
    /// </summary>
    [DisplayName("Unit of Measure CompanyNm")]
    public string UnitOfMeasureNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unit of measure description.
    /// </summary>
    [DisplayName("Unit of Measure Description")]
    public string? UnitOfMeasureDs { get; set; }

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
    /// Gets or sets the collection of questions associated with the unit of measure.
    /// </summary>
    [DisplayName("Questions")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
