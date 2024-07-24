using System.ComponentModel.DataAnnotations;

namespace WebSpark.Core.Data;

public partial class RecipeCategory : BaseEntity
{
    public RecipeCategory()
    {
        Recipe = [];
    }

    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Comment { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public virtual ICollection<Data.Recipe> Recipe { get; set; }
}
