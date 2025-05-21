using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Collections.Generic;

namespace DataSpark.Web.Controllers
{
    public class VisualizationController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public VisualizationController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            // List available CSV files for selection
            var filesPath = Path.Combine(_env.WebRootPath, "files");
            var files = Directory.Exists(filesPath)
                ? Directory.GetFiles(filesPath, "*.csv").Select(Path.GetFileName).Where(f => f != null).Select(f => f!).ToList()
                : new List<string>();
            return View(files);
        }

        [HttpGet]
        public IActionResult Data(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name required");
            var filePath = Path.Combine(_env.WebRootPath, "files", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            var records = csv.GetRecords<dynamic>().ToList();
            var headers = records.Count > 0 ? ((IDictionary<string, object>)records[0]).Keys.ToList() : new List<string>();
            var columns = new Dictionary<string, List<string>>();
            foreach (var header in headers)
            {
                columns[header] = records.Select(r => ((IDictionary<string, object>)r)[header]?.ToString() ?? "").ToList();
            }
            return Json(new { headers, columns });
        }

        public IActionResult Bivariate()
        {
            return View();
        }

        public IActionResult Pivot()
        {
            return View();
        }
    }
}
