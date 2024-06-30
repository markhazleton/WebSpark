using WebSpark.Domain.Entities;
using WebSpark.Domain.Models;

namespace WebSpark.Domain.ViewModels;

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
    public BlogItem? Blog { get; set; }
    public Author Author { get; set; } // posts by author
    public string Category { get; set; } // posts by category
    public IEnumerable<PostItem> Posts { get; set; }
    public Pager Pager { get; set; }
    public PostListType PostListType { get; set; }
    public PostItem Post { get; set; }
    public PostItem Older { get; set; }
    public PostItem Newer { get; set; }
    public IEnumerable<PostItem> Related { get; set; }
}
