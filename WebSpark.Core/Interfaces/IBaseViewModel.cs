namespace WebSpark.Core.Interfaces;


/// <summary>
/// Interface IBaseViewModel
/// </summary>
public interface IBaseViewModel
{
    /// <summary>
    /// Gets or sets the domain identifier.
    /// </summary>
    /// <value>The domain identifier.</value>
    int WebsiteId { get; set; }

    /// <summary>
    /// Gets the menu.
    /// </summary>
    /// <value>The menu.</value>
    List<Models.MenuModel> Menu { get; }

    /// <summary>
    /// Gets or sets the meta description.
    /// </summary>
    /// <value>The meta description.</value>
    string MetaDescription { get; set; }

    /// <summary>
    /// Gets or sets the meta keywords.
    /// </summary>
    /// <value>The meta keywords.</value>
    string MetaKeywords { get; set; }

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    /// <value>The page title.</value>
    string PageTitle { get; set; }

    /// <summary>
    /// Gets or sets the name of the site.
    /// </summary>
    /// <value>The name of the site.</value>
    string WebsiteName { get; set; }

    /// <summary>
    /// Gets or sets the site theme.
    /// </summary>
    /// <value>The site theme.</value>
    string WebsiteStyle { get; set; }

    /// <summary>
    /// Gets or sets the site URL.
    /// </summary>
    /// <value>The site URL.</value>
    Uri SiteUrl { get; set; }
}
