namespace WebSpark.Core.Data;

public partial class RecipeImage : BaseEntity
{
    public string FileName { get; set; }
    public string FileDescription { get; set; }
    public int DisplayOrder { get; set; }
    public byte[] ImageData { get; set; }
    public virtual Recipe Recipe { get; set; }
}
