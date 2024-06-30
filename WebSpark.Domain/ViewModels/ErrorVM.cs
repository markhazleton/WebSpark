
namespace WebSpark.Domain.ViewModels;

/// <summary>
/// Class ErrorViewModel.
/// Implements the <see cref="WebsiteVM" />
/// </summary>
/// <seealso cref="WebsiteVM" />
public class ErrorVM : WebsiteVM
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorVM" /> class.
    /// </summary>
    /// <param name="website">The website.</param>
    public ErrorVM(WebsiteVM website, string slug = null)
    {
        if (website == null) website = new WebsiteVM();

        WebsiteId = website.WebsiteId;
        WebsiteName = website.WebsiteName;
        WebsiteStyle = website.WebsiteStyle;
        CurrentStyle = website.CurrentStyle;
        Template = website.Template;
        SiteUrl = website.SiteUrl;
        MetaDescription = website.MetaDescription;
        MetaKeywords = website.MetaKeywords;
        PageTitle = website.PageTitle;
        Menu = website.Menu;
        StyleUrl = website.StyleUrl;
        Menu = website.Menu;
        StyleList = website.StyleList;
        PageTitle = "Error";
        RequestId = slug;

    }

    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    /// <value>The request identifier.</value>
    public string RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether [show request identifier].
    /// </summary>
    /// <value><c>true</c> if [show request identifier]; otherwise, <c>false</c>.</value>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
