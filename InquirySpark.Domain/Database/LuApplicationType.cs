namespace InquirySpark.Domain.Database;
/// <summary>
/// Lookup table for application types.
/// </summary>
public partial class LuApplicationType
{
    /// <summary>
    /// Gets or sets the application type ID.
    /// </summary>
    [DisplayName("Application Type ID")]
    public int ApplicationTypeId { get; set; }

    /// <summary>
    /// Gets or sets the application type name.
    /// </summary>
    [DisplayName("Application Type CompanyNm")]
    public string ApplicationTypeNm { get; set; } = null!;

    /// <summary>
    /// Gets or sets the application type description.
    /// </summary>
    [DisplayName("Application Type Description")]
    public string? ApplicationTypeDs { get; set; }

    /// <summary>
    /// Gets or sets the modified ID.
    /// </summary>
    [DisplayName("Modified ID")]
    public int ModifiedId { get; set; }

    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    [DisplayName("Modified Date")]
    public DateTime ModifiedDt { get; set; }

    /// <summary>
    /// Gets or sets the collection of applications.
    /// </summary>
    [DisplayName("Applications")]
    public virtual ICollection<Application> Applications { get; set; } = [];
}
