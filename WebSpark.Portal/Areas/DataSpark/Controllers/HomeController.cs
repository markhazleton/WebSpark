using HttpClientUtility.MemoryCache;
using WebSpark.Portal.Areas.DataSpark.Models;
using WebSpark.Portal.Areas.DataSpark.Services;

namespace WebSpark.Portal.Areas.DataSpark.Controllers;

[Area("DataSpark")]
public class HomeController : DataSparkBaseController<HomeController>
{
    private readonly CsvProcessingService _csvProcessingService;

    public HomeController(
        IMemoryCacheManager memoryCacheManager,
        IConfiguration configuration,
        CsvProcessingService csvProcessingService,
        ILogger<HomeController> logger)
        : base(memoryCacheManager, configuration, logger)
    {
        _csvProcessingService = csvProcessingService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // Prepare the initial view model with the list of available CSV files
        var viewModel = new CsvViewModel { AvailableCsvFiles = GetCsvFiles() };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0 || Path.GetExtension(file.FileName)?.ToLower() != ".csv")
        {
            var viewModel = new CsvViewModel
            {
                Message = "Please upload a valid .csv file.",
                AvailableCsvFiles = GetCsvFiles()
            };
            return View("Index", viewModel);
        }

        try
        {
            var filePath = await AddFileAsync(file); // Use AddFileAsync to add and refresh the file list
            if (filePath == null)
            {
                var viewModel = new CsvViewModel
                {
                    Message = "CSV output folder is not configured or file could not be saved.",
                    AvailableCsvFiles = GetCsvFiles()
                };
                return View("Index", viewModel);
            }
            // Process the CSV file and update the corresponding ViewModel in the thread-safe collection
            var processedViewModel = _csvProcessingService.ProcessCsvWithFallback(filePath);
            TryUpdateCsvFile(Path.GetFileName(filePath), processedViewModel);
            return View("Results", processedViewModel);
        }
        catch (Exception ex)
        {
            var viewModel = new CsvViewModel
            {
                Message = $"Unexpected error: {ex.Message}",
                AvailableCsvFiles = GetCsvFiles()
            };
            return View("Index", viewModel);
        }
    }

    [HttpPost]
    public IActionResult ProcessExistingFile(string fileName)
    {
        try
        {
            var filePath = GetFilePath(fileName);

            if (filePath == null)
            {
                var viewModel = new CsvViewModel
                {
                    Message = "Invalid file name or CSV output folder is not configured.",
                    AvailableCsvFiles = GetCsvFiles()
                };
                return View("Index", viewModel);
            }

            // Retrieve the CSV ViewModel from the thread-safe collection or process if not present
            var existingViewModel = GetViewModelForFile(fileName);
            if (existingViewModel == null)
            {
                existingViewModel = _csvProcessingService.ProcessCsvWithFallback(filePath);
                TryUpdateCsvFile(fileName, existingViewModel); // Update the cache in a thread-safe manner
            }

            // Return the results view with the processed ViewModel
            return View("Results", existingViewModel);
        }
        catch (Exception ex)
        {
            var viewModel = new CsvViewModel
            {
                Message = $"Unexpected error: {ex.Message}",
                AvailableCsvFiles = GetCsvFiles()
            };
            return View("Index", viewModel);
        }
    }
}
