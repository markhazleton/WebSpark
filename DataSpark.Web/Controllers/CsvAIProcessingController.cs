using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DataSpark.Web.Controllers;

public class CsvAIProcessingController : Controller
{
    private readonly OpenAIFileAnalysisService _aiService;

    public CsvAIProcessingController(OpenAIFileAnalysisService aiService)
    {
        _aiService = aiService;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return View("Index", "File is empty or not provided.");

        var tempPath = Path.GetTempFileName();
        using (var stream = System.IO.File.Create(tempPath))
            await file.CopyToAsync(stream);

        try
        {
            string analysis = await _aiService.AnalyzeCsvFileAsync(tempPath, "Analyze and summarize this CSV file.");
            ViewBag.Analysis = analysis;
            return View("Index");
        }
        catch (Exception ex)
        {
            // Show the real error message in the view for debugging
            ViewBag.Analysis = $"Error: {ex.Message}";
            return View("Index");
        }
        finally
        {
            // Clean up the temp file
            if (System.IO.File.Exists(tempPath))
            {
                try { System.IO.File.Delete(tempPath); } catch { }
            }
        }
    }
}