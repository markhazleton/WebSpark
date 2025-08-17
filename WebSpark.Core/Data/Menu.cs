namespace WebSpark.Core.Data;

public partial class Menu : BaseEntity
{
    public int DisplayOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string KeyWords { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Argument { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string PageContent { get; set; } = string.Empty;
    public virtual WebSite Domain { get; set; } = null!;
    public virtual Menu? Parent { get; set; }
    public virtual ICollection<Menu> InverseParent { get; set; } = [];
    public virtual ICollection<Keyword> Keywords { get; set; } = [];
}
