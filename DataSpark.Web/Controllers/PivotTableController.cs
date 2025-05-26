using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.MemoryCache;

namespace DataSpark.Web.Controllers;

public class PivotTableController(
        IMemoryCacheManager memoryCacheManager,
        IConfiguration configuration,
        CsvProcessingService csvProcessingService,
        ILogger<PivotTableController> logger,
        CsvFileService csvFileService) : DataSparkBaseController<PivotTableController>(memoryCacheManager, configuration, logger, csvFileService)
{

    /// <summary>
    /// Endpoint to process a CSV file and return the content as a JSON string.
    /// </summary>
    /// <param name="fileName">The name of the CSV file.</param>
    /// <param name="useSafeMethod">Whether to use the safe method for CSV processing (optional, default false).</param>
    /// <returns>A JSON string representing the CSV content or an error message in JSON format.</returns>
    [HttpPost]
    public IActionResult ProcessExistingFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest("File name is required.");
        }
        var filePath = GetFilePath(fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File '{fileName}' not found.");
        }

        try
        {
            // Use the CsvProcessingService to get the CSV content as JSON
            string jsonResult = csvProcessingService.ProcessCsvToJson(filePath, false);
            return View("pivot", jsonResult);
        }
        catch (Exception ex)
        {
            // Return a JSON error message if something goes wrong
            return StatusCode(500, new { error = ex.Message });
        }
    }


    [HttpGet]
    public IActionResult GetExistingFile(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("File name is required.");
        }
        var filePath = GetFilePath(id);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File '{filePath}' not found.");
        }

        try
        {
            // Use the CsvProcessingService to get the CSV content as JSON
            string jsonResult = csvProcessingService.ProcessCsvToJson(filePath, false);
            return Content(jsonResult, "application/json");
        }
        catch (Exception ex)
        {
            // Return a JSON error message if something goes wrong
            return StatusCode(500, new { error = ex.Message });
        }
    }

    public IActionResult fullpage(string filename = "foodhub_order.csv")
    {
        return View("fullpage", filename);
    }

    public IActionResult Index()
    {
        var viewModel = new CsvViewModel { AvailableCsvFiles = GetCsvFiles() };
        return View(viewModel);
    }
}
