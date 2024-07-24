using WebSpark.Core.Providers;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers;

public class RecipeImageFileModel : Core.Models.RecipeImageModel
{
    public IFormFile UploadedImage { get; set; }
}


/// <summary>
/// RecipeImageController
/// </summary>
/// <remarks>
/// RecipeImageController Constructor
/// </remarks>
/// <param name="_logger"></param>
/// <param name="_recipeService"></param>
/// <param name="_recipeImageService"></param>
public class RecipeImageController(
    ILogger<HomeController> _logger,
    Core.Interfaces.IRecipeService _recipeService,
    IRecipeImageService _recipeImageService) : RecipeBaseController
{
    /// <summary>
    /// Index Page
    /// </summary>
    /// <returns></returns>
    public IActionResult Index()
    {
        var recipeImages = _recipeImageService.GetRecipeImages();
        return View(recipeImages);
    }

    /// <summary>
    /// Details page
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IActionResult Details(int id)
    {
        var recipeImage = _recipeImageService.GetRecipeImageById(id);
        if (recipeImage == null)
        {
            return NotFound();
        }

        return View(recipeImage);
    }

    /// <summary>
    /// Create RecipeImage Page
    /// </summary>
    /// <returns></returns>
    public IActionResult Create()
    {
        return View(new RecipeImageFileModel());
    }

    /// <summary>
    /// Post Create RecipeImage 
    /// </summary>
    /// <param name="recipeImageModel"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RecipeImageFileModel recipeImageModel)
    {
        if (recipeImageModel?.UploadedImage != null && recipeImageModel.UploadedImage.Length > 0)
        {
            try
            {
                var dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images");
                Directory.CreateDirectory(dirPath);
                var filePath = Path.Combine(dirPath, recipeImageModel.UploadedImage.FileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await recipeImageModel.UploadedImage.CopyToAsync(stream);

                byte[] fileBytes;
                using (var inputStream = new MemoryStream())
                {
                    // Copy the file stream to the memory stream
                    await recipeImageModel.UploadedImage.CopyToAsync(inputStream);

                    // Convert the memory stream to a byte array
                    fileBytes = inputStream.ToArray();
                    recipeImageModel.ImageData = fileBytes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        recipeImageModel.Recipe = _recipeService.Get(1);

        if (ModelState.IsValid)
        {
            _recipeImageService.AddRecipeImage(recipeImageModel);
            return RedirectToAction(nameof(Index));
        }

        return View(recipeImageModel);
    }

    public IActionResult Edit(int id)
    {
        var recipeImage = _recipeImageService.GetRecipeImageById(id);
        if (recipeImage == null)
        {
            return NotFound();
        }

        return View(recipeImage);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Core.Models.RecipeImageModel recipeImageModel)
    {
        if (id != recipeImageModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _recipeImageService.UpdateRecipeImage(recipeImageModel);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        return View(recipeImageModel);
    }

    public IActionResult Delete(int id)
    {
        var recipeImage = _recipeImageService.GetRecipeImageById(id);
        if (recipeImage == null)
        {
            return NotFound();
        }

        return View(recipeImage);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        _recipeImageService.DeleteRecipeImage(id);
        return RedirectToAction(nameof(Index));
    }
}

