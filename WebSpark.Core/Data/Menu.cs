namespace WebSpark.Core.Data;

public partial class Menu : BaseEntity
{
    public int DisplayOrder { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string KeyWords { get; set; }
    public string Controller { get; set; }
    public string Action { get; set; }
    public string? Argument { get; set; }
    public string Icon { get; set; }
    public string Url { get; set; }
    public string PageContent { get; set; }
    public virtual WebSite Domain { get; set; }
    public virtual Menu? Parent { get; set; }
    public virtual ICollection<Menu> InverseParent { get; set; } = [];
    public virtual ICollection<Keyword> Keywords { get; set; } = [];
}
