using WebSpark.RecipeCookbook;

namespace WebSpark.Portal.Areas.RecipeSpark.Controllers;

public class CookBookController : RecipeBaseController
{
    private readonly ICookbook _cookbook;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _csvOutputFolder;

    public CookBookController(
        ICookbook cookbook,
        IWebHostEnvironment webHostEnvironment,
        IConfiguration configuration)
    {
        _cookbook = cookbook ?? throw new ArgumentNullException(nameof(cookbook));
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));

        // Retrieve the CsvOutputFolder from configuration.
        _csvOutputFolder = configuration["CsvOutputFolder"];
        if (string.IsNullOrWhiteSpace(_csvOutputFolder))
        {
            throw new ArgumentException("CsvOutputFolder is not configured properly.");
        }

        // Ensure that the output directory exists.
        if (!Directory.Exists(_csvOutputFolder))
        {
            Directory.CreateDirectory(_csvOutputFolder);
        }
    }

    public IActionResult Index()
    {
        // Use the CsvOutputFolder from configuration as the output folder.
        string outputPath = Path.Combine(_csvOutputFolder, "Mechanics_of_Motherhood.pdf");

        // Call the MakeCookbook method with the title and description.
        string? path = _cookbook.MakeCookbook(outputPath, "Mechanics of Motherhood", "Recipes from MOM");

        var model = new CookbookViewModel
        {
            // Return a relative path (or you may choose to return the absolute path)
            PdfPath = path != null ? "Mechanics_of_Motherhood.pdf" : null
        };

        return View(model);
    }
}

public class CookbookViewModel
{
    public string PdfPath { get; set; }
}