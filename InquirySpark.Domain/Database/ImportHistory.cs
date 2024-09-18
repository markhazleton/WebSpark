namespace InquirySpark.Domain.Database;

/// <summary>
/// Import history.
/// </summary>
public partial class ImportHistory
{
    /// <summary>
    /// Gets or sets the import history ID.
    /// </summary>
    public int ImportHistoryId { get; set; }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    [DisplayName("File CompanyNm")]
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the import type.
    /// </summary>
    [DisplayName("Import Type")]
    public string ImportType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the number of rows.
    /// </summary>
    [DisplayName("Number of Rows")]
    public int NumberOfRows { get; set; }

    /// <summary>
    /// Gets or sets the import log.
    /// </summary>
    [DisplayName("Import Log")]
    public string? ImportLog { get; set; }

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
}
