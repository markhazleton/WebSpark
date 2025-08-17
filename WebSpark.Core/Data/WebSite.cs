namespace WebSpark.Core.Data;

public partial class WebSite : BaseEntity
{
    [Required]
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string GalleryFolder { get; set; } = string.Empty;
    public string DomainUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool UseBreadCrumbUrl { get; set; }
    public int VersionNo { get; set; }
    public string Style { get; set; } = string.Empty;
    public bool IsRecipeSite { get; set; }
    public virtual ICollection<Menu> Menus { get; set; } = [];
}
