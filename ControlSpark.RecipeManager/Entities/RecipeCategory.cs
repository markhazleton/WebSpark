namespace ControlSpark.RecipeManager.Entities;

public partial class RecipeCategory : BaseEntity
{
    public RecipeCategory()
    {
        Recipe = new HashSet<Recipe>();
    }

    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Comment { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public virtual ICollection<Recipe> Recipe { get; set; }
}
