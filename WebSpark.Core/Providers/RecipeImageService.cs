using WebSpark.Core.Data;

namespace WebSpark.Core.Providers;

public interface IRecipeImageService
{
    void AddRecipeImage(Models.RecipeImageModel recipeImageModel);
    void DeleteRecipeImage(int id);
    Models.RecipeImageModel? GetRecipeImageById(int id);
    IEnumerable<Models.RecipeImageModel> GetRecipeImages();
    void UpdateRecipeImage(Models.RecipeImageModel recipeImageModel);
}

public class RecipeImageService(WebSparkDbContext dbContext) : IRecipeImageService, IDisposable
{
    private bool disposedValue;

    private static RecipeImage ConvertToEntity(Models.RecipeImageModel recipeImageModel, RecipeImage? recipeImage = null)
    {
        recipeImage ??= new RecipeImage();
        recipeImage.FileName = recipeImageModel.FileName;
        recipeImage.FileDescription = recipeImageModel.FileDescription;
        recipeImage.DisplayOrder = recipeImageModel.DisplayOrder;
        recipeImage.Id = recipeImageModel.Recipe.Id;
        recipeImage.ImageData = recipeImageModel.ImageData;
        return recipeImage;
    }

    private static Models.RecipeImageModel ConvertToModel(RecipeImage recipeImage)
    {
        return new Models.RecipeImageModel
        {
            Id = recipeImage.Id,
            FileName = recipeImage.FileName,
            FileDescription = recipeImage.FileDescription,
            DisplayOrder = recipeImage.DisplayOrder,
            ImageData = recipeImage.ImageData,
            Recipe = new Models.RecipeModel
            {
                Id = recipeImage.Recipe?.Id ?? 0,
                Name = recipeImage.Recipe?.Name ?? string.Empty,
                Description = recipeImage.Recipe?.Description ?? string.Empty
            }

        };
    }

    public void AddRecipeImage(Models.RecipeImageModel recipeImageModel)
    {
        var recipeImage = ConvertToEntity(recipeImageModel);
        dbContext.RecipeImage.Add(recipeImage);
        dbContext.SaveChanges();
    }

    public void DeleteRecipeImage(int id)
    {
        var recipeImage = dbContext.RecipeImage.SingleOrDefault(r => r.Id == id);
        if (recipeImage != null)
        {
            dbContext.RecipeImage.Remove(recipeImage);
            dbContext.SaveChanges();
        }
    }

    public Models.RecipeImageModel? GetRecipeImageById(int id)
    {
        var recipeImage = dbContext.RecipeImage
            .Include(r => r.Recipe)
            .SingleOrDefault(r => r.Id == id);
        return recipeImage != null ? ConvertToModel(recipeImage) : null;
    }

    public IEnumerable<Models.RecipeImageModel> GetRecipeImages()
    {
        var recipeImages = dbContext.RecipeImage
            .Include(r => r.Recipe)
            .OrderBy(r => r.DisplayOrder)
            .ToList();
        return recipeImages.Select(r => ConvertToModel(r));
    }

    public void UpdateRecipeImage(Models.RecipeImageModel recipeImageModel)
    {
        var existingRecipeImage = dbContext.RecipeImage.SingleOrDefault(r => r.Id == recipeImageModel.Id) ?? throw new InvalidOperationException("Recipe image not found.");
        ConvertToEntity(recipeImageModel, existingRecipeImage);
        dbContext.SaveChanges();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (dbContext != null)
                    dbContext.Dispose();
            }
            disposedValue = true;
        }
    }
    ~RecipeImageService()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
