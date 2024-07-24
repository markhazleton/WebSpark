namespace WebSpark.Core.Models;

public class ListModel
{
    public Models.BlogItem Blog { get; set; }
    public Models.AuthorItem Author { get; set; } // posts by author
    public string Category { get; set; } // posts by category
    public IEnumerable<Models.PostItem> Posts { get; set; }
    public Pager Pager { get; set; }
    public PostListType PostListType { get; set; }
}
