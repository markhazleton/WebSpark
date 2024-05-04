using WebSpark.Domain.Entities;

namespace WebSpark.RecipeManager.Entities;

public class RecipeOld
{
    public RecipeOld()
    {
    }

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
    public string RecipeNM { get; set; }
    public string Description { get; set; }
    public string AuthorNM { get; set; }
    public string Ingredients { get; set; }
    public string Instructions { get; set; }
    public string IsApproved { get; set; }
    public DateTime ModifiedDT { get; set; }
    public int ModifiedID { get; set; }
    public int ViewCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime LastViewDT { get; set; }
}
