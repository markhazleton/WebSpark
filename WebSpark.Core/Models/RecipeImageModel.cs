namespace WebSpark.Core.Models
{
    public class RecipeImageModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileDescription { get; set; }
        public int DisplayOrder { get; set; }
        public byte[] ImageData { get; set; }
        public Models.RecipeModel Recipe { get; set; } = new Models.RecipeModel();
    }
}
