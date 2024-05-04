using WebSpark.Domain.Entities;
using WebSpark.RecipeManager.Entities;

namespace WebSpark.Core.Helpers;

public static class RecipeHelper
{
    public static RecipeCategory GetRecipeCategory(string CategoryName, int displayOrder)
    {
        var myCat = new RecipeCategory()
        {
            DisplayOrder = displayOrder,
            IsActive = true,
            Comment = CategoryName,
            Name = CategoryName
        };
        return myCat;
    }
    public static Recipe GetRecipe(WebSite domain, string name, string authorName, string description, string ingredients, string instructions, RecipeCategory category)
    {
        return new Recipe()
        {
            Name = name,
            AuthorName = authorName,
            Description = description,
            Ingredients = ingredients,
            Instructions = instructions,
            Domain = domain,
            RecipeCategory = category,
        };
    }
}
