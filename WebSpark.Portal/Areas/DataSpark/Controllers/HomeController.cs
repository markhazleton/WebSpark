
using WebSpark.Portal.Areas.DataSpark.Models;
using WebSpark.Portal.Areas.DataSpark.Services;

namespace WebSpark.Portal.Areas.DataSpark.Controllers;

public class HomeController(
    IConfiguration configuration,
    CsvProcessingService csvProcessingService,
    ILogger<HomeController> logger) : DataSparkBaseController<HomeController>(logger)
{
    [HttpGet]
    public IActionResult Index()
    {
        var outputFolder = configuration["CsvOutputFolder"];
        var csvFiles = new List<string>();

        if (!string.IsNullOrEmpty(outputFolder) && Directory.Exists(outputFolder))
        {
            // Get all CSV files from the output folder
            csvFiles = Directory.GetFiles(outputFolder, "*.csv")
                                .Select(file => Path.GetFileName(file)!)
                                .ToList();
        }
        var viewModel = new CsvViewModel
        {
            AvailableCsvFiles = csvFiles
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0 || Path.GetExtension(file.FileName)?.ToLower() != ".csv")
        {
            return View("Index", new CsvViewModel { Message = "Please upload a valid .csv file." });
        }

        try
        {
            // Get output folder from configuration
            var outputFolder = configuration["CsvOutputFolder"];

            if (string.IsNullOrEmpty(outputFolder))
            {
                return View("Index", new CsvViewModel { Message = "CSV output folder is not configured." });
            }

            // Ensure the directory exists
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // Set file name and path
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(outputFolder, fileName);

            // Save the file to the specified path
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Process the CSV file using the service
            var model = csvProcessingService.ProcessCsvWithFallback(filePath);

            return View("Results", model);
        }
        catch (Exception ex)
        {
            return View("Index", new CsvViewModel { Message = $"Unexpected error: {ex.Message}" });
        }
    }

    [HttpPost]
    public IActionResult ProcessExistingFile(string fileName)
    {
        try
        {
            // Get output folder from configuration
            var outputFolder = configuration["CsvOutputFolder"];

            if (string.IsNullOrEmpty(outputFolder) || string.IsNullOrEmpty(fileName))
            {
                return View("Index", new CsvViewModel { Message = "Invalid file name or CSV output folder is not configured." });
            }

            // Ensure the output folder exists
            if (!Directory.Exists(outputFolder))
            {
                return View("Index", new CsvViewModel { Message = "The specified output folder does not exist." });
            }

            // Get all files in the directory, ignoring case
            var files = Directory.GetFiles(outputFolder, "*", SearchOption.TopDirectoryOnly)
                .Select(f => new FileInfo(f))
                .ToList();

            // Find the file with case-insensitive comparison
            var fileInfo = files.FirstOrDefault(f => string.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase));

            // Check if the file exists
            if (fileInfo == null)
            {
                return View("Index", new CsvViewModel { Message = "The specified file does not exist." });
            }

            // Construct the full file path using the matched file
            var filePath = fileInfo.FullName;

            // Process the CSV file using the service
            var model = csvProcessingService.ProcessCsvWithFallback(filePath);

            // Return the results view
            return View("Results", model);
        }
        catch (Exception ex)
        {
            return View("Index", new CsvViewModel { Message = $"Unexpected error: {ex.Message}" });
        }
    }
}
