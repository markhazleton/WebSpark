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
        InitEditModel();
    }

    private void InitEditModel()
    {
        DisplayOrder = 1;
        DisplayInNavigation = true;
        Controllers =
        [
            new LookupModel { Text = "Page", Value = "Page", IsDefault = true, IsSelected = true },
            new LookupModel { Text = "Blog", Value = "Blog", IsDefault = false, IsSelected = false },
            new LookupModel { Text = "Recipe", Value = "Recipe", IsDefault = false, IsSelected = false },
            new LookupModel { Text = "Bootswatch", Value = "Bootswatch", IsDefault = false, IsSelected = false }
        ];
        Domains = [];
        Parents = [];
        Icons =
        [
            new LookupModel { Text="Home", Value="fa-home" },
            new LookupModel { Text="Heart", Value="fa-heart" },
            new LookupModel { Text="Chevron >", Value="fa-chevron-right" },
            new LookupModel { Text="Chevron <", Value="fa-chevron-left" },
            new LookupModel { Text="Cog/Gears", Value="fa-cog" },
            new LookupModel { Text="Bolt", Value="fa-bolt" },
            new LookupModel { Text="Comment", Value="fa-comment" },
            new LookupModel { Text="Coffee", Value="fa-coffee" },
            new LookupModel { Text="Cutlery", Value="fa-cutlery" }
        ];
        Actions =
        [
            new LookupModel { Text = "Index", Value = "index" },
            new LookupModel { Text = "Categories (Blog)", Value = "categories" }
        ];
    }

    public MenuEditModel(MenuModel? menu)
    {
        InitEditModel();

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

    public List<LookupModel> Domains { get; set; }
    public List<LookupModel> Parents { get; set; }
    public List<LookupModel> Controllers { get; set; }
    public List<LookupModel> Icons { get; set; }
    public List<LookupModel> Actions { get; set; }
}
