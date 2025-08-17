namespace WebSpark.Core.Models;

public class PostModel
{
    public BlogItem Blog { get; set; } = new();
    public PostItem Post { get; set; } = new();
    public PostItem Older { get; set; } = new();
    public PostItem Newer { get; set; } = new();
    public IEnumerable<PostItem> Related { get; set; } = new List<PostItem>();
}
