using System.Diagnostics;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using DataSpark.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataSpark.Web.Controllers
{
    public class HomeController(IWebHostEnvironment env, ILogger<HomeController> logger) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.ErrorMessage = "Please upload a valid CSV file.";
                return View("Index");
            }

            try
            {
                // Save the file to wwwroot/files
                var uploadPath = Path.Combine(env.WebRootPath, "files");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Process the saved file
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
                var records = csv.GetRecords<dynamic>().ToList();

                if (!records.Any())
                {
                    ViewBag.ErrorMessage = "The CSV file is empty.";
                    return View("Index");
                }

                // Example: Display the first 10 records (or any processing result)
                ViewBag.Message = "CSV file uploaded and saved successfully!";
                ViewBag.Records = records.Take(10).ToList(); // Display the first 10 records
                ViewBag.FilePath = $"/files/{file.FileName}";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred while processing the file: {ex.Message}";
                return View("Index");
            }

            return View("Index");
        }

        public IActionResult Files()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
