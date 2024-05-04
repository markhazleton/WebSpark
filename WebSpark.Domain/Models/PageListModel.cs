
namespace WebSpark.Domain.Models;

public class PageListModel
{
    public IEnumerable<PostItem> Posts { get; set; }
    public Pager Pager { get; set; }
}
