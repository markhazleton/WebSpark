using ControlSpark.RecipeManager.Models;

namespace ControlSpark.RecipeManager.EditModels
{
    /// <summary>
    /// Class RecipeModel.
    /// </summary>
    public class RecipeEditModel : RecipeModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecipeModel" /> class.
        /// </summary>
        public RecipeEditModel()
        {
        }
        public RecipeEditModel(RecipeModel? Recipe)
        {
            if (Recipe == null) return;
            AuthorNM = Recipe.AuthorNM;
            AverageRating = Recipe.AverageRating;
            IsApproved = Recipe.IsApproved;
            CommentCount = Recipe.CommentCount;
            Description = Recipe.Description;
            FileDescription = Recipe.FileDescription;
            LastViewDT = Recipe.LastViewDT;
            FileName = Recipe.FileName;
            Id = Recipe.Id;
            ModifiedDT = Recipe.ModifiedDT;
            RecipeCategoryID = Recipe.RecipeCategoryID;
            Ingredients = Recipe.Ingredients;
            Instructions = Recipe.Instructions;
            ModifiedID = Recipe.ModifiedID;
            Servings = Recipe.Servings;
            Name = Recipe.Name;
            RatingCount = Recipe.RatingCount;
            RecipeCategories = Recipe.RecipeCategories;
            RecipeCategory = Recipe.RecipeCategory;
            RecipeCategoryNM = Recipe.RecipeCategoryNM;
            RecipeURL = Recipe.RecipeURL;
            ViewCount = Recipe.ViewCount;
        }
        public List<RecipeCategoryModel> Categories { get; set; } = new List<RecipeCategoryModel>();

    }
}
