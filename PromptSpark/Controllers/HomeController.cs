using Markdig;
using PromptSpark.Models;
using System.Diagnostics;

namespace PromptSpark.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _env;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }
    public IActionResult KitchenSink()
    {
        var filePath = Path.Combine(_env.WebRootPath, "kitchen_sink.html");
        var htmlContent = System.IO.File.ReadAllText(filePath);
        return View((object)htmlContent);
    }
    public IActionResult PageDisplay(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Filename cannot be empty.");
        }

        // Construct the full file path
        var filePath = Path.Combine(_env.WebRootPath, "markdown", $"{id}.md");

        // Check if the file exists
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"The file {id}.md was not found.");
        }

        try
        {
            // Read the contents of the Markdown file
            var markdownContent = System.IO.File.ReadAllText(filePath);

            // Convert Markdown to HTML using Markdig
            var htmlContent = Markdown.ToHtml(markdownContent);

            // Pass the HTML content to the View
            return View("PageDisplay", model: htmlContent);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it accordingly
            return StatusCode(500, $"An error occurred while processing your request. Message:{ex.Message}");
        }
    }

    public IActionResult Index(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return View();
        }

        // Construct the full file path
        var filePath = Path.Combine(_env.WebRootPath, "markdown", $"{id}.md");

        // Check if the file exists
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"The file {id}.md was not found.");
        }

        try
        {
            // Read the contents of the Markdown file
            var markdownContent = System.IO.File.ReadAllText(filePath);

            // Convert Markdown to HTML using Markdig
            var htmlContent = Markdown.ToHtml(markdownContent);

            // Pass the HTML content to the View
            return View("PageDisplay", model: htmlContent);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it accordingly
            return StatusCode(500, $"An error occurred while processing your request. Message:{ex.Message}");
        }



    }
    public IActionResult TestOnPageNavigation()
    {
        return View();
    }
    public IActionResult LearnMore()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
