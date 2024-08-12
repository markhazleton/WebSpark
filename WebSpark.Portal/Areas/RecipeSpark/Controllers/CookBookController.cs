using WebSpark.RecipeCookbook;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers;

public class CookBookController(ICookbook cookbook) : RecipeBaseController
{
    public IActionResult Index()
    {
        string? path = cookbook.MakeCookbook();
        var model = new CookbookViewModel
        {
            PdfPath = path
        };
        return View(model);
    }
}
public class CookbookViewModel
{
    public string PdfPath { get; set; }
}
