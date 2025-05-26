using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DataSpark.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HomeController> _logger;
        private readonly CsvFileService _csvFileService;
        private readonly CsvProcessingService _csvProcessingService;

        public HomeController(IWebHostEnvironment env, ILogger<HomeController> logger, CsvFileService csvFileService, CsvProcessingService csvProcessingService)
        {
            _env = env;
            _logger = logger;
            _csvFileService = csvFileService;
            _csvProcessingService = csvProcessingService;
        }
        public IActionResult Index()
        {
            _logger.LogInformation("Index page accessed.");
            // Pass available CSV files to the view for dropdowns
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> UploadCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                ViewBag.ErrorMessage = "Please upload a valid CSV file.";
                var files = _csvFileService.GetCsvFileNames();
                return View("Index", files);
            }

            try
            {
                // Use the CSV file service to save the file
                var savedFileName = await _csvFileService.SaveUploadedFileAsync(file);

                if (savedFileName == null)
                {
                    ViewBag.ErrorMessage = "Failed to save the uploaded file.";
                    var files = _csvFileService.GetCsvFileNames();
                    return View("Index", files);
                }

                // Read and process the saved file using the service
                var csvData = _csvFileService.ReadCsvForVisualization(savedFileName);

                if (!csvData.Records.Any())
                {
                    ViewBag.ErrorMessage = "The CSV file is empty.";
                    var files = _csvFileService.GetCsvFileNames();
                    return View("Index", files);
                }

                // Display success message and sample records
                ViewBag.Message = "CSV file uploaded and saved successfully!";
                ViewBag.Records = csvData.Records.Take(10).ToList(); // Display the first 10 records
                ViewBag.FilePath = $"/files/{savedFileName}";
                var filesList = _csvFileService.GetCsvFileNames();
                return View("Index", filesList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the file.");
                ViewBag.ErrorMessage = $"An error occurred while processing the file: {ex.Message}";
                var files = _csvFileService.GetCsvFileNames();
                return View("Index", files);
            }
        }

        public IActionResult Files()
        {
            // Pass available CSV files to the view for dropdowns
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            // Pass available CSV files to the view for dashboard
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }

        public async Task<IActionResult> Results(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                ViewBag.ErrorMessage = "No file specified.";
                var files = _csvFileService.GetCsvFileNames();
                return View("Index", files);
            }

            // Use the processing service to build a full analysis view model
            var model = await _csvProcessingService.ProcessCsvWithFallbackAsync(fileName);
            if (model == null || model.RowCount == 0)
            {
                ViewBag.ErrorMessage = "The selected CSV file is empty or could not be read.";
                var files = _csvFileService.GetCsvFileNames();
                return View("Index", files);
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
