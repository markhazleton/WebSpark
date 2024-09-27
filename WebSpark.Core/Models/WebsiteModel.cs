namespace WebSpark.Core.Models;

/// <summary>
/// Class WebSiteModel.
/// </summary>
public class WebsiteModel
{
    /// <summary>
    /// 
    /// </summary>
    public WebsiteModel()
    {
        Menu = [];
        Message = null;
    }

    /// <summary>
    /// Gets or sets the website description.
    /// </summary>
    /// <value>The domain ds.</value>
    [Required]
    [StringLength(250)]
    public string Description { get; set; }
    /// <summary>
    /// Gets or sets the website identifier.
    /// </summary>
    /// <value>The website identifier.</value>
    public int Id { get; set; }

    /// <summary>
    /// The List of Menu Items for this domain
    /// </summary>
    public List<MenuModel> Menu { get; set; }
    /// <summary>
    /// Message for processing this object (not saved in database)
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the modified dt.
    /// </summary>
    /// <value>The modified dt.</value>
    public DateTime ModifiedDT { get; set; }

    /// <summary>
    /// Gets or sets the modified identifier.
    /// </summary>
    /// <value>The modified identifier.</value>
    public int ModifiedID { get; set; }

    /// <summary>
    /// Gets or sets the website name.
    /// </summary>
    /// <value>The domain nm.</value>
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the gallery folder.
    /// </summary>
    /// <value>The gallery folder.</value>
    [Required]
    [StringLength(20)]
    [Display(Name = "Site Name")]
    public string SiteName { get; set; }
    /// <summary>
    /// Name of the Style
    /// </summary>
    [Display(Name = "Site Style")]
    public string SiteStyle { get; set; }

    /// <summary>
    /// Gets or sets the website code.
    /// </summary>
    /// <value>The domain comment.</value>
    [Required]
    [StringLength(20)]
    [Display(Name = "Site Template")]
    public string SiteTemplate { get; set; }
    /// <summary>
    /// Link to API for this website
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [use bread crumb URL].
    /// </summary>
    /// <value><c>true</c> if [use bread crumb URL]; otherwise, <c>false</c>.</value>
    public bool UseBreadCrumbURL { get; set; }

    /// <summary>
    /// Gets or sets the version no.
    /// </summary>
    /// <value>The version no.</value>
    public int VersionNo { get; set; }

    /// <summary>
    /// Gets or sets the domain title.
    /// </summary>
    /// <value>The domain title.</value>
    [Required]
    [StringLength(250)]
    public string WebsiteTitle { get; set; }

    /// <summary>
    /// Gets or sets the domain URL.
    /// </summary>
    /// <value>The domain URL.</value>
    [Required]
    [StringLength(250)]
    public string WebsiteUrl { get; set; }

    /// <summary>
    /// Is this a recipe site?
    /// </summary>
    [Required]
    public bool IsRecipeSite { get; set; } = false;

}
