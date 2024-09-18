namespace InquirySpark.Domain.Database;

public partial class AppProperty
{
    /// <summary>
    /// Gets or sets the ID of the AppProperty.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the SiteApp associated with the AppProperty.
    /// </summary>
    public int SiteAppId { get; set; }

    /// <summary>
    /// Gets or sets the key of the AppProperty.
    /// </summary>
    [DisplayName("Key")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the value of the AppProperty.
    /// </summary>
    [DisplayName("Value")]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the SiteApp associated with the AppProperty.
    /// </summary>
    public virtual Application SiteApp { get; set; } = null!;
}
