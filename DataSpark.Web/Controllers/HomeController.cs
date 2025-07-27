using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DataSpark.Web.Controllers;

public class BaseController : Controller
{
    protected readonly IWebHostEnvironment _env;
    protected readonly ILogger _logger;
    protected readonly CsvFileService _csvFileService;
    protected readonly CsvProcessingService _csvProcessingService;

    public BaseController(IWebHostEnvironment env, ILogger logger, CsvFileService csvFileService, CsvProcessingService csvProcessingService)
    {
        _env = env;
        _logger = logger;
        _csvFileService = csvFileService;
        _csvProcessingService = csvProcessingService;
    }

    // Protected method for handling common file validation logic
    protected IActionResult? HandleFileNotFound(string fileName, string errorMessage = "No file specified.")
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return ReturnToIndexWithError(errorMessage);
        }
        return null;
    }

    // Protected method for handling empty CSV files
    protected IActionResult? HandleEmptyCsv(CsvViewModel model, string fileName)
    {
        if (model == null || model.RowCount == 0)
        {
            return ReturnToIndexWithError("The selected CSV file is empty or could not be read.");
        }
        return null;
    }

    // Protected method for handling file upload errors
    protected IActionResult? HandleFileUploadError(IFormFile file, string errorMessage = "Please upload a valid CSV file.")
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("No file uploaded.");
            return ReturnToIndexWithError(errorMessage);
        }
        return null;
    }

    // Protected method for handling file save failures
    protected IActionResult? HandleFileSaveFailure(string? savedFileName, string errorMessage = "Failed to save the uploaded file.")
    {
        if (savedFileName == null)
        {
            return ReturnToIndexWithError(errorMessage);
        }
        return null;
    }

    // Protected method for handling empty CSV data
    protected IActionResult? HandleEmptyCsvData(IEnumerable<dynamic> records, string errorMessage = "The CSV file is empty.")
    {
        if (!records.Any())
        {
            return ReturnToIndexWithError(errorMessage);
        }
        return null;
    }

    // Protected method for handling exceptions with logging
    protected IActionResult HandleException(Exception ex, string errorMessagePrefix = "An error occurred while processing the file")
    {
        _logger.LogError(ex, errorMessagePrefix);
        return ReturnToIndexWithError($"{errorMessagePrefix}: {ex.Message}");
    }

    // Protected method for setting up successful file upload response
    protected IActionResult SetupSuccessfulUploadResponse(string savedFileName, IEnumerable<dynamic> records, int recordsToShow = 10)
    {
        ViewBag.Message = "CSV file uploaded and saved successfully!";
        ViewBag.Records = records.Take(recordsToShow).ToList();
        ViewBag.FilePath = $"/files/{savedFileName}";
        var filesList = _csvFileService.GetCsvFileNames();
        return View("Index", filesList);
    }

    // Protected method for getting files list and returning to Index view with error
    protected IActionResult ReturnToIndexWithError(string errorMessage)
    {
        ViewBag.ErrorMessage = errorMessage;
        var files = _csvFileService.GetCsvFileNames();
        return View("Index", files);
    }

    // Protected method for getting files list and returning to Index view
    protected IActionResult ReturnToIndexWithFiles(object? model = null)
    {
        var files = _csvFileService.GetCsvFileNames();
        return View("Index", model ?? files);
    }
}


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
