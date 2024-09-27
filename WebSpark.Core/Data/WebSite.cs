namespace WebSpark.Core.Data;

public partial class WebSite : BaseEntity
{
    [Required]
    public required string Name { get; set; }
    public string Description { get; set; }
    public string Template { get; set; }
    public string GalleryFolder { get; set; }
    public string DomainUrl { get; set; }
    public string Title { get; set; }
    public bool UseBreadCrumbUrl { get; set; }
    public int VersionNo { get; set; }
    public string Style { get; set; }
    public bool IsRecipeSite { get; set; }
    public virtual ICollection<Menu> Menus { get; set; } = [];
}
