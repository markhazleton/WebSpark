using ControlSpark.Domain.Interfaces;
using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.ViewModels;

/// <summary>
/// Class BaseViewModel.
/// Implements the <see cref="IBaseViewModel" />
/// </summary>
/// <seealso cref="IBaseViewModel" />
public class WebsiteVM : IBaseViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebsiteVM" /> class.
    /// </summary>
    public WebsiteVM()
    {
    }

    /// <summary>
    /// Gets the menu.
    /// </summary>
    /// <value>The menu.</value>
    [JsonPropertyName("menus")]
    public List<MenuModel> Menu { get; set; }

    /// <summary>
    /// Gets or sets the meta description.
    /// </summary>
    /// <value>The meta description.</value>
    [JsonPropertyName("meta_description")]
    public string MetaDescription { get; set; }

    /// <summary>
    /// Gets or sets the meta keywords.
    /// </summary>
    /// <value>The meta keywords.</value>
    [JsonPropertyName("meta_keywords")]
    public string MetaKeywords { get; set; }

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    /// <value>The page title.</value>
    [JsonPropertyName("page_title")]
    public string PageTitle { get; set; }
    /// <summary>
    /// Gets or sets the site URL.
    /// </summary>
    /// <value>The site URL.</value>
    [JsonPropertyName("site_url")]
    public Uri SiteUrl { get; set; }
    /// <summary>
    /// Gets The List of Themes
    /// </summary>
    public IEnumerable<StyleModel> StyleList { get; set; }

    /// <summary>
    /// Gets or sets the theme URL.
    /// </summary>
    /// <value>The site URL.</value>
    [JsonPropertyName("style_url")]
    public Uri StyleUrl { get; set; }


    /// <summary>
    /// Gets or sets the website code
    /// </summary>
    public string Template { get; set; }

    /// <summary>
    /// Gets or sets the website identifier.
    /// </summary>
    /// <value>The website identifier.</value>
    [JsonPropertyName("website_id")]
    public int WebsiteId { get; set; }

    /// <summary>
    /// Gets or sets the name of the site.
    /// </summary>
    /// <value>The name of the site.</value>
    [JsonPropertyName("website_name")]
    public string WebsiteName { get; set; }

    /// <summary>
    /// Gets or sets the website theme.
    /// </summary>
    /// <value>The website theme.</value>
    [JsonPropertyName("website_style")]
    public string WebsiteStyle { get; set; }

    public string HeaderScript { get; set; }
    public string FooterScript { get; set; }
    public string PageCanonical { get; set; }


}
