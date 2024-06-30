using WebSpark.Bootswatch.Model;

namespace WebSpark.Domain.ViewModels;
/// <summary>
/// Class ThemeVM.
/// Implements the <see cref="WebsiteVM" />
/// </summary>
/// <seealso cref="WebsiteVM" />
public class ThemeVM : WebsiteVM
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeVM" /> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    public ThemeVM(WebsiteVM parent)
    {
        Menu.Clear();
        Menu.AddRange(parent.Menu);
        WebsiteId = parent.WebsiteId;
        WebsiteName = parent.WebsiteName;
        WebsiteStyle = parent.WebsiteStyle;
        SiteUrl = parent.SiteUrl;
        StyleList = [];
        Theme = new StyleModel();
    }

    /// <summary>
    /// Gets or sets the theme.
    /// </summary>
    /// <value>The theme.</value>
    public StyleModel Theme { get; set; }
}
