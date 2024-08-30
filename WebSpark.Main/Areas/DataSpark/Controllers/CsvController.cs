using WebSpark.Main.Areas.DataSpark.Models;
using WebSpark.Main.Areas.DataSpark.Services;

namespace WebSpark.Main.Areas.DataSpark.Controllers
{
    public class CsvController(
        IConfiguration configuration, 
        CsvProcessingService csvProcessingService) : DataSparkBaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new CsvViewModel());
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
    }
}
