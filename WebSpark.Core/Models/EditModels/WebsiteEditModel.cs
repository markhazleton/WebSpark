namespace WebSpark.Core.Models.EditModels;


/// <summary>
/// Class WebsiteEditModel.
/// </summary>
public class WebsiteEditModel : WebsiteModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebsiteModel" /> class.
    /// </summary>
    public WebsiteEditModel()
    {
    }
    public WebsiteEditModel(WebsiteModel website)
    {
        if (website == null) return;
        Message = website.Message;
        SiteStyle = website.SiteStyle;
        Id = website.Id;
        Name = website.Name;
        Description = website.Description;
        SiteTemplate = website.SiteTemplate;
        SiteName = website.SiteName;
        WebsiteUrl = website.WebsiteUrl;
        WebsiteTitle = website.WebsiteTitle;
        UseBreadCrumbURL = website.UseBreadCrumbURL;
        ModifiedID = website.ModifiedID;
        ModifiedDT = website.ModifiedDT;
        VersionNo = website.VersionNo;
        IsRecipeSite = website.IsRecipeSite;
        Url = website.Url;
        Menu = website.Menu.ToList();
    }
}
