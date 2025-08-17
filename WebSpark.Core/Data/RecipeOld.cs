namespace WebSpark.Core.Data;

public class RecipeOld
{
    public Recipe GetRecipe(WebSite mom, RecipeCategory rcat)
    {
        return new Recipe()
        {
            Id = Id,
            Name = RecipeNM,
            Description = Description,
            Ingredients = Ingredients,
            Instructions = Instructions,
            AuthorName = AuthorNM,
            AverageRating = AverageRating,
            CommentCount = 0,
            CreatedDate = ModifiedDT,
            CreatedID = 1,
            Domain = mom,
            IsApproved = true,
            LastViewDt = ModifiedDT,
            RatingCount = 0,
            RecipeCategory = rcat
        };
    }

    public int Id { get; set; }
    public int RecipeCategoryID { get; set; }
    public string RecipeNM { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuthorNM { get; set; } = string.Empty;
    public string Ingredients { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string IsApproved { get; set; } = string.Empty;
    public DateTime ModifiedDT { get; set; }
    public int ModifiedID { get; set; }
    public int ViewCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime LastViewDT { get; set; }
}
