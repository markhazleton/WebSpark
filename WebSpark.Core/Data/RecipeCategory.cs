namespace WebSpark.Core.Data;

public partial class RecipeCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public virtual WebSite Domain { get; set; } = null!;
    public virtual ICollection<Recipe> Recipe { get; set; } = [];
}
