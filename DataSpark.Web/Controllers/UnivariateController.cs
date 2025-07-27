using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataSpark.Web.Controllers;

public class UnivariateController : BaseController
{
    public UnivariateController(IWebHostEnvironment env, ILogger<UnivariateController> logger, CsvFileService csvFileService, CsvProcessingService csvProcessingService)
        : base(env, logger, csvFileService, csvProcessingService)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? fileName)
    {
        var files = _csvFileService.GetCsvFileNames();
        if (string.IsNullOrEmpty(fileName) && files.Any())
            fileName = files.First();

        if (string.IsNullOrEmpty(fileName))
        {
            ViewBag.ErrorMessage = "No CSV files found. Please upload a file first.";
            ViewBag.Files = files;
            return View(new UnivariateViewModel { FileName = string.Empty });
        }

        try
        {
            // Use CsvProcessingService to generate a full univariate analysis
            var csvModel = await _csvProcessingService.ProcessCsvWithFallbackAsync(fileName);

            var model = new UnivariateViewModel
            {
                FileName = fileName,
                AvailableColumns = csvModel.ColumnDetails.Select(c => c.Column).ToList(),
                Message = csvModel.Message
            };

            ViewBag.Files = files;
            ViewBag.CsvModel = csvModel; // Pass the full analysis to the view

            return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"Error analyzing file: {ex.Message}";
            ViewBag.Files = files;
            return View(new UnivariateViewModel { FileName = fileName });
        }
    }

    [HttpPost]
    public IActionResult Analyze(string fileName, string columnName)
    {
        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(columnName))
        {
            return BadRequest("File name and column name are required.");
        }

        try
        {
            // Read the CSV data
            var csvData = _csvFileService.ReadCsvForVisualization(fileName);

            if (!csvData.Records.Any())
            {
                return BadRequest("No data found in the CSV file.");
            }

            // Convert to DataTable for visualization
            var dataTable = ConvertToDataTable(csvData.Records);

            // Generate SVG visualization
            var svgContent = _csvProcessingService.GetScottPlotSvg(columnName, dataTable);

            return Content(svgContent, "image/svg+xml");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating analysis: {ex.Message}");
        }
    }

    private static System.Data.DataTable ConvertToDataTable(List<dynamic> records)
    {
        var dataTable = new System.Data.DataTable();

        if (records.Any())
        {
            var first = records.First() as IDictionary<string, object>;
            if (first != null)
            {
                // Add columns
                foreach (var kvp in first)
                {
                    dataTable.Columns.Add(kvp.Key, typeof(object));
                }

                // Add rows
                foreach (var record in records)
                {
                    var dict = record as IDictionary<string, object>;
                    if (dict != null)
                    {
                        var row = dataTable.NewRow();
                        foreach (var kvp in dict)
                        {
                            row[kvp.Key] = kvp.Value ?? DBNull.Value;
                        }
                        dataTable.Rows.Add(row);
                    }
                }
            }
        }

        return dataTable;
    }
}