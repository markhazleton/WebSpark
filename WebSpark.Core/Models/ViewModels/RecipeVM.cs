namespace WebSpark.Core.Models.ViewModels;

/// <summary>
/// Class RecipeVM.
/// Implements the <see cref="WebsiteVM" />
/// </summary>
/// <seealso cref="WebsiteVM" />
public class RecipeVM : WebsiteVM
{
    /// <summary>
    /// 
    /// </summary>
    public RecipeVM()
    {
        Recipe = new();
        Category = new();
        CategoryList = [];
        RecipeList = [];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="website"></param>
    public RecipeVM(WebsiteVM website)
    {
        WebsiteId = website.WebsiteId;
        WebsiteName = website.WebsiteName;
        WebsiteStyle = website.WebsiteStyle;
        CurrentStyle = website.CurrentStyle;
        IsRecipeSite = website.IsRecipeSite;
        Template = website.Template;
        SiteUrl = website.SiteUrl;
        MetaDescription = website.MetaDescription;
        MetaKeywords = website.MetaKeywords;
        PageTitle = website.PageTitle;
        Menu = website.Menu;
        StyleList = website.StyleList;
        StyleUrl = website.StyleUrl;
        Recipe = new();
        Category = new();
        CategoryList = [];
        RecipeList = [];
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("category")]
    public RecipeCategoryModel Category { get; set; }
    /// <summary>
    /// Gets the category list.
    /// </summary>
    /// <value>The category list.</value>
    [JsonPropertyName("category_list")]
    public List<RecipeCategoryModel> CategoryList { get; set; }

    /// <summary>
    /// Gets or sets the recipe.
    /// </summary>
    /// <value>The recipe.</value>
    [JsonPropertyName("recipe")]
    public RecipeModel Recipe { get; set; }
    /// <summary>
    /// Gets the recipe list.
    /// </summary>
    /// <value>The recipe list.</value>
    [JsonPropertyName("recipes")]
    public List<RecipeModel> RecipeList { get; set; }
}
