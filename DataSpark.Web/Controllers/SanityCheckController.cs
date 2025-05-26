using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataSpark.Web.Controllers;

public class SanityCheckController : Controller
{
    private readonly CsvFileService _csvFileService;
    public SanityCheckController(CsvFileService csvFileService)
    {
        _csvFileService = csvFileService;
    }

    public IActionResult Index(string? fileName)
    {
        var files = _csvFileService.GetCsvFileNames();
        if (string.IsNullOrEmpty(fileName) && files.Any())
            fileName = files.First();

        if (string.IsNullOrEmpty(fileName))
        {
            ViewBag.ErrorMessage = "No CSV files found. Please upload a file first.";
            return View(new Models.SanityCheckViewModel { AvailableFiles = files });
        }

        var csvData = _csvFileService.ReadCsvForVisualization(fileName);
        var model = new Models.SanityCheckViewModel
        {
            FileName = fileName,
            AvailableFiles = files,
            Headers = csvData.Headers,
            RowCount = csvData.Records.Count,
            SampleRows = csvData.Records.Take(5).ToList(),
            HasMissingValues = csvData.Records.Any(r => ((IDictionary<string, object>)r).Values.Any(v => v == null || string.IsNullOrEmpty(v?.ToString())))
        };
        return View(model);
    }
}

public class SanityCheckViewModel
{
    public string? FileName { get; set; }
    public List<string> AvailableFiles { get; set; } = new();
    public List<string> Headers { get; set; } = new();
    public int RowCount { get; set; }
    public List<dynamic> SampleRows { get; set; } = new();
    public bool HasMissingValues { get; set; }
}
