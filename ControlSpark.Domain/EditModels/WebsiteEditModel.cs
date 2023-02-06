using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.EditModels;


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
    public WebsiteEditModel(WebsiteModel? website)
    {
        if (website == null) return;
        Message = website.Message;
        Theme = website.Theme;
        Id = website.Id;
        Name = website.Name;
        Description = website.Description;
        Template = website.Template;
        GalleryFolder = website.GalleryFolder;
        WebsiteUrl = website.WebsiteUrl;
        WebsiteTitle = website.WebsiteTitle;
        UseBreadCrumbURL = website.UseBreadCrumbURL;
        ModifiedID = website.ModifiedID;
        ModifiedDT = website.ModifiedDT;
        VersionNo = website.VersionNo;
        Url = website.Url;

    }
}
