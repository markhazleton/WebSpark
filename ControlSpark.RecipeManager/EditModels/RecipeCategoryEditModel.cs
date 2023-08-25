using ControlSpark.RecipeManager.Models;

namespace ControlSpark.RecipeManager.EditModels;

public class RecipeCategoryEditModel : RecipeCategoryModel
{
    public RecipeCategoryEditModel()
    {

    }
    public RecipeCategoryEditModel(RecipeCategoryModel model)
    {
        if (model == null) return;
        Id = model.Id;
        Name = model.Name;
        Description = model.Description;
        DisplayOrder = model.DisplayOrder;
        IsActive = model.IsActive;
        Recipes = model.Recipes;
        Url = model.Url;

    }
}
