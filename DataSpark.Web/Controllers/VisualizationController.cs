using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DataSpark.Web.Controllers
{
    public class VisualizationController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly CsvFileService _csvFileService;

        public VisualizationController(IWebHostEnvironment env, CsvFileService csvFileService)
        {
            _env = env;
            _csvFileService = csvFileService;
        }

        public IActionResult Index()
        {
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }

        [HttpGet]
        public IActionResult Data(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name required");

            if (!_csvFileService.FileExists(fileName))
                return NotFound("File not found");

            // Use the CSV file service to read data for visualization
            var csvData = _csvFileService.ReadCsvForVisualization(fileName);
            return Json(new { headers = csvData.Headers, columns = csvData.Columns });
        }

        [HttpGet]
        public IActionResult Bivariate()
        {
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }

        [HttpGet]
        public IActionResult Pivot()
        {
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }

        [HttpGet]
        public IActionResult Univariate()
        {
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }

        [HttpGet]
        public IActionResult FullAnalysisReport()
        {
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var files = _csvFileService.GetCsvFileNames();
            return View(files);
        }
    }
}
