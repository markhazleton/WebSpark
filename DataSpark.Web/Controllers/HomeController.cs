using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DataSpark.Web.Controllers;


public class HomeController : BaseController
{
    public HomeController(IWebHostEnvironment env, ILogger<HomeController> logger, CsvFileService csvFileService, CsvProcessingService csvProcessingService)
        : base(env, logger, csvFileService, csvProcessingService)
    {
    }
    public IActionResult Index()
    {
        _logger.LogInformation("Index page accessed.");
        // Pass available CSV files to the view for dropdowns
        return ReturnToIndexWithFiles();
    }

    [HttpPost]
    public async Task<IActionResult> UploadCSV(IFormFile file)
    {
        // Use the base controller method for file validation
        var fileError = HandleFileUploadError(file);
        if (fileError != null) return fileError;

        try
        {
            // Use the CSV file service to save the file
            var savedFileName = await _csvFileService.SaveUploadedFileAsync(file);

            // Check if file save failed
            var saveError = HandleFileSaveFailure(savedFileName);
            if (saveError != null) return saveError;

            // Read and process the saved file using the service
            var csvData = _csvFileService.ReadCsvForVisualization(savedFileName!);

            // Check if CSV data is empty
            var emptyError = HandleEmptyCsvData(csvData.Records);
            if (emptyError != null) return emptyError;

            // Return success response
            return SetupSuccessfulUploadResponse(savedFileName!, csvData.Records);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error processing the file");
        }
    }

    public IActionResult Files()
    {
        // Pass available CSV files to the view for dropdowns
        return ReturnToIndexWithFiles();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> CompleteAnalysis(string fileName)
    {
        // Use the base controller method for file validation
        var fileError = HandleFileNotFound(fileName);
        if (fileError != null) return fileError;

        // Use the processing service to build a full analysis view model
        var model = await _csvProcessingService.ProcessCsvWithFallbackAsync(fileName);

        // Use the base controller method for empty CSV handling
        var emptyError = HandleEmptyCsv(model, fileName);
        if (emptyError != null) return emptyError;

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
