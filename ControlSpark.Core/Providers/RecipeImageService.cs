namespace ControlSpark.Core.Providers;

public interface IRecipeImageService
{
    void AddRecipeImage(RecipeImageModel recipeImageModel);
    void DeleteRecipeImage(int id);
    RecipeImageModel GetRecipeImageById(int id);
    IEnumerable<RecipeImageModel> GetRecipeImages();
    void UpdateRecipeImage(RecipeImageModel recipeImageModel);
}

public class RecipeImageService : IRecipeImageService, IDisposable
{
    private readonly AppDbContext _dbContext;

    public RecipeImageService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<RecipeImageModel> GetRecipeImages()
    {
        var recipeImages = _dbContext.RecipeImage
            .Include(r => r.Recipe)
            .OrderBy(r => r.DisplayOrder)
            .ToList();

        return recipeImages.Select(r => ConvertToModel(r));
    }

    public RecipeImageModel GetRecipeImageById(int id)
    {
        var recipeImage = _dbContext.RecipeImage
            .Include(r => r.Recipe)
            .SingleOrDefault(r => r.Id == id);

        return recipeImage != null ? ConvertToModel(recipeImage) : null;
    }

    public void AddRecipeImage(RecipeImageModel recipeImageModel)
    {
        var recipeImage = ConvertToEntity(recipeImageModel);
        _dbContext.RecipeImage.Add(recipeImage);
        _dbContext.SaveChanges();
    }

    public void UpdateRecipeImage(RecipeImageModel recipeImageModel)
    {
        var existingRecipeImage = _dbContext.RecipeImage.SingleOrDefault(r => r.Id == recipeImageModel.Id);
        if (existingRecipeImage == null)
        {
            throw new InvalidOperationException("Recipe image not found.");
        }

        ConvertToEntity(recipeImageModel, existingRecipeImage);
        _dbContext.SaveChanges();
    }

    public void DeleteRecipeImage(int id)
    {
        var recipeImage = _dbContext.RecipeImage.SingleOrDefault(r => r.Id == id);
        if (recipeImage != null)
        {
            _dbContext.RecipeImage.Remove(recipeImage);
            _dbContext.SaveChanges();
        }
    }

    private RecipeImageModel ConvertToModel(RecipeImage recipeImage)
    {
        return new RecipeImageModel
        {
            Id = recipeImage.Id,
            FileName = recipeImage.FileName,
            FileDescription = recipeImage.FileDescription,
            DisplayOrder = recipeImage.DisplayOrder,
            ImageData = recipeImage.ImageData,
            Recipe = new RecipeModel
            {
                Id = recipeImage.Recipe.Id,
                Name = recipeImage.Recipe.Name,
                Description = recipeImage.Recipe.Description
            }

        };
    }

    private RecipeImage ConvertToEntity(RecipeImageModel recipeImageModel, RecipeImage recipeImage = null)
    {
        recipeImage ??= new RecipeImage();
        recipeImage.FileName = recipeImageModel.FileName;
        recipeImage.FileDescription = recipeImageModel.FileDescription;
        recipeImage.DisplayOrder = recipeImageModel.DisplayOrder;
        recipeImage.Id = recipeImageModel.Recipe.Id;
        recipeImage.ImageData = recipeImageModel.ImageData;
        return recipeImage;
    }

    public void Dispose()
    {
        ((IDisposable)_dbContext).Dispose();
    }
}
