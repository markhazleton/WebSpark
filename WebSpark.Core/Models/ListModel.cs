namespace WebSpark.Core.Models;

public class ListModel
{
    public Models.BlogItem Blog { get; set; } = new();
    public Models.AuthorItem Author { get; set; } = new(); // posts by author
    public string Category { get; set; } = string.Empty; // posts by category
    public IEnumerable<Models.PostItem> Posts { get; set; } = new List<Models.PostItem>();
    public Pager Pager { get; set; } = new(1);
    public PostListType PostListType { get; set; }
}
