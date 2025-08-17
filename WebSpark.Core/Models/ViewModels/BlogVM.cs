namespace WebSpark.Core.Models.ViewModels;

/// <summary>
/// Class RecipeVM.
/// Implements the <see cref="WebsiteVM" />
/// </summary>
/// <seealso cref="WebsiteVM" />
public class BlogVM : WebsiteVM
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="website"></param>
    public BlogVM(WebsiteVM website)
    {
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
        StyleList = website.StyleList;
        StyleUrl = website.StyleUrl;
        PostListType = PostListType.Blog;
    }
    public BlogItem Blog { get; set; } = new();
    public AuthorItem Author { get; set; } = new(); // posts by author
    public string Category { get; set; } = string.Empty; // posts by category
    public IEnumerable<PostItem> Posts { get; set; } = Array.Empty<PostItem>();
    public Pager Pager { get; set; } = new(currentPage: 1);
    public PostListType PostListType { get; set; }
    public PostItem Post { get; set; } = new();
    public PostItem Older { get; set; } = new();
    public PostItem Newer { get; set; } = new();
    public IEnumerable<PostItem> Related { get; set; } = Array.Empty<PostItem>();
}
