using WebSpark.RecipeCookbook;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers;

public class CookBookController(ICookbook cookbook, IWebHostEnvironment webHostEnvironment) : RecipeBaseController
{
    private readonly ICookbook _cookbook = cookbook ?? throw new ArgumentNullException(nameof(cookbook));
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));

    public IActionResult Index()
    {
        // Get the path for the wwwroot/docs directory
        string docsPath = Path.Combine(_webHostEnvironment.WebRootPath, "docs");

        // Ensure the directory exists
        if (!Directory.Exists(docsPath))
        {
            Directory.CreateDirectory(docsPath);
        }

        // Specify the output path for the PDF
        string outputPath = Path.Combine(docsPath, "Mechanics_of_Motherhood.pdf");

        // Call the MakeCookbook method with the title and description
        string? path = _cookbook.MakeCookbook(outputPath, "Mechanics of Motherhood", "Recipes from MOM");

        var model = new CookbookViewModel
        {
            // Set the relative path correctly
            PdfPath = path != null ? "docs/Mechanics_of_Motherhood.pdf" : null
        };

        return View(model);
    }
}

public class CookbookViewModel
{
    public string PdfPath { get; set; }
}
