using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.EditModels;

/// <summary>
/// Class MenuModel.
/// </summary>
public class MenuEditModel : MenuModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuModel" /> class.
    /// </summary>
    public MenuEditModel()
    {
        DisplayOrder = 1;
        DisplayInNavigation = true;
    }
    public MenuEditModel(MenuModel? menu)
    {
        DisplayOrder = 1;
        DisplayInNavigation = true;

        if (menu == null) return;
        Action = menu.Action;
        Argument = menu.Argument;
        ApiUrl = menu.ApiUrl;
        Controller = menu.Controller;
        Description = menu.Description;
        DisplayInNavigation = menu.DisplayInNavigation;
        DisplayOrder = menu.DisplayOrder;
        DomainID = menu.DomainID;
        DomainUrl = menu.DomainUrl;
        DomainName = menu.DomainName;
        Icon = menu.Icon;
        Id = menu.Id;
        LastModified = menu.LastModified;
        PageContent = menu.PageContent;
        ParentController = menu.ParentController;
        ParentId = menu.ParentId;
        ParentTitle = menu.ParentTitle;
        Title = menu.Title;
        Url = menu.Url;
        VirtualPath = menu.VirtualPath;
    }

    public List<WebsiteModel> Domains { get; set; } = new List<WebsiteModel>();

}
