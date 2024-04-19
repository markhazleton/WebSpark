using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.ViewModels;
/// <summary>
/// Class PageVM.
/// Implements the <see cref="WebsiteVM" />
/// </summary>
/// <seealso cref="WebsiteVM" />
public class PageVM : WebsiteVM
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuVM" /> class.
    /// </summary>
    public PageVM()
    {
        Item = new MenuModel();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuVM" /> class.
    /// </summary>
    /// <param name="domain">The parent.</param>
    public PageVM(WebsiteVM domain)
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
        Template = domain.Template;
        Item = new MenuModel();
        PageTitle = "Menu Maintenance";
    }

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    /// <value>The item.</value>
    public MenuModel Item { get; set; }

}

/// <summary>
/// Class MenuVM.
/// Implements the <see cref="WebsiteVM" />
/// </summary>
/// <seealso cref="WebsiteVM" />
public class MenuVM : WebsiteVM
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuVM" /> class.
    /// </summary>
    public MenuVM()
    {
        Item = new MenuModel();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuVM" /> class.
    /// </summary>
    /// <param name="domain">The parent.</param>
    public MenuVM(WebsiteVM domain)
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
        Item = new MenuModel();
        ItemCollection = new List<MenuModel>();
        PageTitle = "Menu Maintenance";
    }

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    /// <value>The item.</value>
    public MenuModel Item { get; set; }

    /// <summary>
    /// Gets the item collection.
    /// </summary>
    /// <value>The item collection.</value>
    public List<MenuModel> ItemCollection { get; }
}
