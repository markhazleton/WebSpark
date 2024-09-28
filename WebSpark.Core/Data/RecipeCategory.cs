namespace WebSpark.Core.Data;

public partial class RecipeCategory : BaseEntity
{
    public string Name { get; set; }
    public string Comment { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public virtual WebSite Domain { get; set; }
    public virtual ICollection<Recipe> Recipe { get; set; } = [];
}
