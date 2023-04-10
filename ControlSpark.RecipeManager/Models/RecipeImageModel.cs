namespace ControlSpark.RecipeManager.Models
{
    public class RecipeImageModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileDescription { get; set; }
        public int DisplayOrder { get; set; }
        public RecipeModel Recipe { get; set; } = new RecipeModel();
    }
}
