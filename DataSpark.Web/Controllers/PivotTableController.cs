using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace DataSpark.Web.Controllers;

public class PivotTableController : BaseController
{
    public PivotTableController(
        IWebHostEnvironment env,
        ILogger<PivotTableController> logger,
        CsvFileService csvFileService,
        CsvProcessingService csvProcessingService)
        : base(env, logger, csvFileService, csvProcessingService)
    {
    }

    [HttpGet]
    public IActionResult Index()
    {
        try
        {
            var model = BuildPivotTableViewModel();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pivot table interface");
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }

    [HttpGet]
    public IActionResult FullPage()
    {
        var model = BuildPivotTableViewModel();
        return View(model);
    }

    [HttpGet]
    public IActionResult SelectColumn()
    {
        var model = BuildPivotTableViewModel();
        return View(model);
    }

    [HttpGet]
    public IActionResult Results(string? fileName, List<string>? columns = null)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = HttpContext.Session.GetString("CurrentCsvFile");
        }

        if (string.IsNullOrEmpty(fileName))
        {
            return RedirectToAction("SelectColumn");
        }

        try
        {
            var records = _csvFileService.ReadCsvRecords(fileName);
            if (!records.Any())
            {
                return RedirectToAction("SelectColumn");
            }

            var firstRecord = (IDictionary<string, object>)records.First();
            var model = new PivotTableViewModel
            {
                CurrentFile = fileName,
                ColumnHeaders = columns?.Any() == true ? columns : firstRecord.Keys.ToList(),
                RecordCount = records.Count,
                AvailableFiles = _csvFileService.GetCsvFileNames()
            };

            HttpContext.Session.SetString("CurrentCsvFile", fileName);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pivot table results for file {FileName}", fileName);
            return RedirectToAction("SelectColumn");
        }
    }

    [HttpPost]
    public IActionResult LoadCsvData([FromBody] LoadCsvDataRequest request)
    {
        var response = ProcessLoadCsvDataRequest(request);
        return Json(response);
    }

    [HttpPost]
    public IActionResult SaveConfiguration([FromBody] SaveConfigurationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                return Json(new StandardResponse
                {
                    Success = false,
                    Error = "Configuration name is required"
                });
            }

            var config = new PivotTableConfiguration
            {
                Name = request.Name,
                Description = request.Description,
                CsvFile = request.CsvFile,
                AggregatorName = request.AggregatorName,
                RendererName = request.RendererName,
                Cols = request.Cols,
                Rows = request.Rows,
                Vals = request.Vals,
                IncludeValues = request.IncludeValues,
                ExcludeValues = request.ExcludeValues,
                CreatedAt = DateTime.Now
            };

            // For now, we'll just log the configuration
            // In a real application, you'd save this to a database
            _logger.LogInformation("Pivot table configuration saved: {@Config}", config);

            return Json(new StandardResponse
            {
                Success = true,
                Message = "Configuration saved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving pivot table configuration");
            return Json(new StandardResponse
            {
                Success = false,
                Error = $"Error saving configuration: {ex.Message}"
            });
        }
    }

    [HttpPost]
    public IActionResult Export(string format, string configuration)
    {
        try
        {
            if (string.IsNullOrEmpty(format) || string.IsNullOrEmpty(configuration))
            {
                return BadRequest("Format and configuration are required");
            }

            var config = JsonSerializer.Deserialize<PivotTableConfiguration>(configuration);
            if (config == null)
            {
                return BadRequest("Invalid configuration");
            }

            // Load the CSV data
            var records = _csvFileService.ReadCsvRecords(config.CsvFile);
            if (!records.Any())
            {
                return BadRequest("No data available for export");
            }

            // Convert to export format
            var exportData = ConvertCsvToJsonData(records);
            var fileName = $"pivot_table_export_{DateTime.Now:yyyyMMdd_HHmmss}";

            switch (format.ToLower())
            {
                case "csv":
                    return ExportAsCsv(exportData, $"{fileName}.csv");
                case "tsv":
                    return ExportAsTsv(exportData, $"{fileName}.tsv");
                case "json":
                    return ExportAsJson(exportData, config, $"{fileName}.json");
                case "excel":
                    return ExportAsExcel(exportData, $"{fileName}.xlsx");
                default:
                    return BadRequest("Unsupported export format");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting pivot table data");
            return BadRequest($"Export failed: {ex.Message}");
        }
    }
}
