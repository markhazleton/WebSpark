using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.ViewModels;

/// <summary>
/// 
/// </summary>
public class WebsiteAdminVM : WebsiteVM
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuVM" /> class.
    /// </summary>
    public WebsiteAdminVM()
    {
        Item = new WebsiteModel();
        ItemCollection = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuVM" /> class.
    /// </summary>
    /// <param name="domain">The parent.</param>
    public WebsiteAdminVM(WebsiteVM domain)
    {
        if (domain is null)
        {
            domain = new WebsiteVM();
        }
        WebsiteId = domain.WebsiteId;
        WebsiteName = domain.WebsiteName;
        WebsiteStyle = domain.WebsiteStyle;
        SiteUrl = domain.SiteUrl;
        MetaDescription = domain.MetaDescription;
        MetaKeywords = domain.MetaKeywords;
        PageTitle = domain.PageTitle;
        Menu = domain.Menu;
        StyleList = domain.StyleList;
        StyleUrl = domain.StyleUrl;

        Item = new WebsiteModel();
        ItemCollection = new List<WebsiteModel>();
        PageTitle = "Website Maintenance";
    }

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    /// <value>The item.</value>
    public WebsiteModel Item { get; set; }

    /// <summary>
    /// Gets the item collection.
    /// </summary>
    /// <value>The item collection.</value>
    public List<WebsiteModel> ItemCollection { get; }

}
