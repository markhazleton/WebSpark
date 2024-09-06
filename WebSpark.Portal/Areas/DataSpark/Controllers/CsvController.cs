using Microsoft.AspNetCore.Mvc;
using WebSpark.Portal.Areas.DataSpark.Models;
using WebSpark.Portal.Areas.DataSpark.Services;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebSpark.Portal.Areas.DataSpark.Controllers;


public class CsvController(
    IConfiguration configuration,
    CsvProcessingService csvProcessingService) : DataSparkBaseController
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
                                .Select(Path.GetFileName)
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

            // Construct full file path
            var filePath = Path.Combine(outputFolder, fileName);

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return View("Index", new CsvViewModel { Message = "The specified file does not exist." });
            }

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
