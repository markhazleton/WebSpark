namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a chart setting.
/// </summary>
public partial class ChartSetting
{
    /// <summary>
    /// Gets or sets the ID of the chart setting.
    /// </summary>
    [DisplayName("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the site user.
    /// </summary>
    [DisplayName("Site User ID")]
    public int SiteUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the site app.
    /// </summary>
    [DisplayName("Site App ID")]
    public int SiteAppId { get; set; }

    /// <summary>
    /// Gets or sets the type of the setting.
    /// </summary>
    [DisplayName("Setting Type")]
    public string SettingType { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the setting.
    /// </summary>
    [DisplayName("Setting CompanyNm")]
    public string SettingName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the value of the setting.
    /// </summary>
    [DisplayName("Setting Value")]
    public string SettingValue { get; set; } = null!;

    /// <summary>
    /// Gets or sets the enhanced value of the setting.
    /// </summary>
    [DisplayName("Setting Value Enhanced")]
    public string? SettingValueEnhanced { get; set; }

    /// <summary>
    /// Gets or sets the date when the chart setting was created.
    /// </summary>
    [DisplayName("Date Created")]
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// Gets or sets the last updated date of the chart setting.
    /// </summary>
    [DisplayName("Last Updated")]
    public DateTime LastUpdated { get; set; }
}
