using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public IActionResult Index()
    {
        // Initialize the form model with default values
        var model = new CookbookViewModel
        {
            Title = "Mechanics of Motherhood",
            Description = "Recipes from MOM",
            Filename = "Mechanics_of_Motherhood.pdf"
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Index(CookbookViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Sanitize filename to avoid invalid characters
        string sanitizedFilename = SanitizeFilename(model.Filename);
        if (!sanitizedFilename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            sanitizedFilename += ".pdf";
        }

        // Create the output path
        string outputPath = Path.Combine(_csvOutputFolder, sanitizedFilename);

        // Call the MakeCookbook method with the title and description from the form
        string? generatedPath = _cookbook.MakeCookbook(outputPath, model.Title, model.Description);

        // Update the model to indicate success or failure
        model.PdfPath = generatedPath != null ? sanitizedFilename : null;
        model.IsGenerated = generatedPath != null;

        // Return to the view with the updated model
        return View(model);
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters
    /// </summary>
    private string SanitizeFilename(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return "cookbook.pdf";

        // Remove invalid characters
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string sanitized = new string(filename.Where(c => !invalidChars.Contains(c)).ToArray());

        // Replace spaces with underscores
        sanitized = sanitized.Replace(' ', '_');

        return sanitized;
    }

    [HttpGet]
    public IActionResult Download(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            return BadRequest("Filename is required");
        }

        string filePath = Path.Combine(_csvOutputFolder, filename);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found");
        }

        // Return the file for download
        return PhysicalFile(filePath, "application/pdf", filename);
    }
}

public class CookbookViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public string? PdfPath { get; set; }
    public bool IsGenerated { get; set; }
}