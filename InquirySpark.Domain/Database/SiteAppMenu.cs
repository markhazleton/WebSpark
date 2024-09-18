namespace InquirySpark.Domain.Database;

/// <summary>
/// Represents a site application menu.
/// </summary>
public partial class SiteAppMenu
{
    /// <summary>
    /// Gets or sets the ID of the menu.
    /// </summary>
    [DisplayName("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the site application.
    /// </summary>
    [DisplayName("Site Application ID")]
    public int SiteAppId { get; set; }

    /// <summary>
    /// Gets or sets the text of the menu.
    /// </summary>
    [DisplayName("Menu Text")]
    public string MenuText { get; set; } = null!;

    /// <summary>
    /// Gets or sets the target page of the menu.
    /// </summary>
    [DisplayName("Target Page")]
    public string TartgetPage { get; set; } = null!;

    /// <summary>
    /// Gets or sets the glyph name of the menu.
    /// </summary>
    [DisplayName("Glyph CompanyNm")]
    public string GlyphName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the order of the menu.
    /// </summary>
    [DisplayName("Menu Order")]
    public int MenuOrder { get; set; }

    /// <summary>
    /// Gets or sets the ID of the site role.
    /// </summary>
    [DisplayName("Site Role ID")]
    public int SiteRoleId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the menu should be displayed in the menu.
    /// </summary>
    [DisplayName("View In Menu")]
    public bool ViewInMenu { get; set; }

    /// <summary>
    /// Gets or sets the site application associated with the menu.
    /// </summary>
    [DisplayName("Site Application")]
    public virtual Application SiteApp { get; set; } = null!;
}
