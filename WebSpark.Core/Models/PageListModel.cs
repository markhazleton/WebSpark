namespace WebSpark.Core.Models;

public class PageListModel
{
    public IEnumerable<PostItem> Posts { get; set; } = new List<PostItem>();
    public Pager Pager { get; set; } = new(1);
}
