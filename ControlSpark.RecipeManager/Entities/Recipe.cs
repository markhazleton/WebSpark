namespace ControlSpark.RecipeManager.Entities;

public partial class Recipe : BaseEntity
{
    public Recipe()
    {
        RecipeComment = new HashSet<RecipeComment>();
        RecipeImage = new HashSet<RecipeImage>();
    }

    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string AuthorName { get; set; }
    public int Servings { get; set; }
    public string Ingredients { get; set; }
    public string Instructions { get; set; }
    public bool IsApproved { get; set; }
    public int ViewCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime LastViewDt { get; set; }
    public virtual WebSite Domain { get; set; }
    public virtual RecipeCategory RecipeCategory { get; set; }
    public virtual ICollection<RecipeComment> RecipeComment { get; set; }
    public virtual ICollection<RecipeImage> RecipeImage { get; set; }
}
