namespace WebSpark.Core.Models
{
    public class RecipeImageModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileDescription { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public Models.RecipeModel Recipe { get; set; } = new Models.RecipeModel();
    }
}
