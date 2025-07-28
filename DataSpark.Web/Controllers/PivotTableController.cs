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
            var model = new PivotTableViewModel
            {
                AvailableFiles = _csvFileService.GetCsvFileNames(),
                CurrentFile = HttpContext.Session.GetString("CurrentCsvFile") ?? string.Empty
            };

            if (!string.IsNullOrEmpty(model.CurrentFile))
            {
                try
                {
                    var records = _csvFileService.ReadCsvRecords(model.CurrentFile);
                    if (records.Any())
                    {
                        var firstRecord = (IDictionary<string, object>)records.First();
                        model.ColumnHeaders = firstRecord.Keys.ToList();
                        model.RecordCount = records.Count;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load current file {FileName}", model.CurrentFile);
                }
            }

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
        var model = new PivotTableViewModel
        {
            AvailableFiles = _csvFileService.GetCsvFileNames(),
            CurrentFile = HttpContext.Session.GetString("CurrentCsvFile") ?? string.Empty
        };

        if (!string.IsNullOrEmpty(model.CurrentFile))
        {
            try
            {
                var records = _csvFileService.ReadCsvRecords(model.CurrentFile);
                if (records.Any())
                {
                    var firstRecord = (IDictionary<string, object>)records.First();
                    model.ColumnHeaders = firstRecord.Keys.ToList();
                    model.RecordCount = records.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load current file {FileName}", model.CurrentFile);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult SelectColumn()
    {
        var model = new PivotTableViewModel
        {
            AvailableFiles = _csvFileService.GetCsvFileNames(),
            CurrentFile = HttpContext.Session.GetString("CurrentCsvFile") ?? string.Empty
        };

        if (!string.IsNullOrEmpty(model.CurrentFile))
        {
            try
            {
                var records = _csvFileService.ReadCsvRecords(model.CurrentFile);
                if (records.Any())
                {
                    var firstRecord = (IDictionary<string, object>)records.First();
                    model.ColumnHeaders = firstRecord.Keys.ToList();
                    model.RecordCount = records.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load current file {FileName}", model.CurrentFile);
            }
        }

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
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FileName))
            {
                return Json(new LoadCsvDataResponse
                {
                    Success = false,
                    Error = "File name is required"
                });
            }

            var records = _csvFileService.ReadCsvRecords(request.FileName);
            if (!records.Any())
            {
                return Json(new LoadCsvDataResponse
                {
                    Success = false,
                    Error = "File not found or contains no data"
                });
            }

            var processedData = ConvertCsvToJsonData(records, request.SelectedColumns);
            var firstRecord = (IDictionary<string, object>)records.First();
            var columns = request.SelectedColumns?.Any() == true
                ? request.SelectedColumns
                : firstRecord.Keys.ToList();

            HttpContext.Session.SetString("CurrentCsvFile", request.FileName);

            return Json(new LoadCsvDataResponse
            {
                Success = true,
                Data = processedData,
                Columns = columns,
                RecordCount = processedData.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CSV data for file: {FileName}", request?.FileName);
            return Json(new LoadCsvDataResponse
            {
                Success = false,
                Error = "Failed to load CSV data: " + ex.Message
            });
        }
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

    private List<Dictionary<string, object>> ConvertCsvToJsonData(
        List<dynamic> records,
        List<string>? selectedColumns = null)
    {
        var result = new List<Dictionary<string, object>>();

        foreach (var record in records)
        {
            var rowDict = new Dictionary<string, object>();
            var dynamicRecord = (IDictionary<string, object>)record;

            foreach (var kvp in dynamicRecord)
            {
                if (selectedColumns == null || selectedColumns.Contains(kvp.Key))
                {
                    rowDict[kvp.Key] = kvp.Value ?? string.Empty;
                }
            }
            result.Add(rowDict);
        }

        return result;
    }

    private IActionResult ExportAsCsv(List<Dictionary<string, object>> data, string fileName)
    {
        if (!data.Any()) return BadRequest("No data to export");

        var csv = new System.Text.StringBuilder();
        var headers = data.First().Keys.ToList();

        // Add headers
        csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

        // Add data rows
        foreach (var row in data)
        {
            var values = headers.Select(h => $"\"{row.GetValueOrDefault(h, string.Empty)}\"");
            csv.AppendLine(string.Join(",", values));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", fileName);
    }

    private IActionResult ExportAsTsv(List<Dictionary<string, object>> data, string fileName)
    {
        if (!data.Any()) return BadRequest("No data to export");

        var tsv = new System.Text.StringBuilder();
        var headers = data.First().Keys.ToList();

        // Add headers
        tsv.AppendLine(string.Join("\t", headers));

        // Add data rows
        foreach (var row in data)
        {
            var values = headers.Select(h => row.GetValueOrDefault(h, string.Empty)?.ToString() ?? string.Empty);
            tsv.AppendLine(string.Join("\t", values));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(tsv.ToString());
        return File(bytes, "text/tab-separated-values", fileName);
    }

    private IActionResult ExportAsJson(List<Dictionary<string, object>> data, PivotTableConfiguration config, string fileName)
    {
        var exportObject = new
        {
            Data = data,
            Configuration = config,
            ExportedAt = DateTime.Now,
            RecordCount = data.Count
        };

        var json = JsonSerializer.Serialize(exportObject, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", fileName);
    }

    private IActionResult ExportAsExcel(List<Dictionary<string, object>> data, string fileName)
    {
        // For now, export as CSV since we don't have Excel library
        // In a real application, you'd use libraries like EPPlus or ClosedXML
        return ExportAsCsv(data, fileName.Replace(".xlsx", ".csv"));
    }
}
