using DataSpark.Web.Models;
using DataSpark.Web.Models.Chart;
using DataSpark.Web.Services;
using DataSpark.Web.Services.Chart;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DataSpark.Web.Controllers;

public class BaseController : Controller
{
    protected readonly IWebHostEnvironment _env;
    protected readonly ILogger _logger;
    protected readonly CsvFileService _csvFileService;
    protected readonly CsvProcessingService _csvProcessingService;

    // Chart services (optional for controllers that don't need them)
    protected readonly IChartService? _chartService;
    protected readonly IDataService? _dataService;
    protected readonly IChartRenderingService? _renderingService;
    protected readonly IChartValidationService? _validationService;

    // Constructor for CSV-only controllers (like HomeController)
    public BaseController(IWebHostEnvironment env, ILogger logger,
        CsvFileService csvFileService, CsvProcessingService csvProcessingService)
    {
        _env = env;
        _logger = logger;
        _csvFileService = csvFileService;
        _csvProcessingService = csvProcessingService;
    }

    // Constructor for Chart controllers (like ChartController)
    public BaseController(IWebHostEnvironment env, ILogger logger,
        CsvFileService csvFileService, CsvProcessingService csvProcessingService,
        IChartService chartService, IDataService dataService,
        IChartRenderingService renderingService, IChartValidationService validationService)
        : this(env, logger, csvFileService, csvProcessingService)
    {
        _chartService = chartService;
        _dataService = dataService;
        _renderingService = renderingService;
        _validationService = validationService;
    }

    // Protected method for handling common file validation logic
    protected IActionResult? HandleFileNotFound(string fileName, string errorMessage = "No file specified.")
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return ReturnToIndexWithError(errorMessage);
        }
        return null;
    }

    // Protected method for handling empty CSV files
    protected IActionResult? HandleEmptyCsv(CsvViewModel model, string fileName)
    {
        if (model == null || model.RowCount == 0)
        {
            return ReturnToIndexWithError("The selected CSV file is empty or could not be read.");
        }
        return null;
    }

    // Protected method for handling file upload errors
    protected IActionResult? HandleFileUploadError(IFormFile file, string errorMessage = "Please upload a valid CSV file.")
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("No file uploaded.");
            return ReturnToIndexWithError(errorMessage);
        }
        return null;
    }

    // Protected method for handling file save failures
    protected IActionResult? HandleFileSaveFailure(string? savedFileName, string errorMessage = "Failed to save the uploaded file.")
    {
        if (savedFileName == null)
        {
            return ReturnToIndexWithError(errorMessage);
        }
        return null;
    }

    // Protected method for handling empty CSV data
    protected IActionResult? HandleEmptyCsvData(IEnumerable<dynamic> records, string errorMessage = "The CSV file is empty.")
    {
        if (!records.Any())
        {
            return ReturnToIndexWithError(errorMessage);
        }
        return null;
    }

    // Protected method for handling exceptions with logging
    protected IActionResult HandleException(Exception ex, string errorMessagePrefix = "An error occurred while processing the file")
    {
        _logger.LogError(ex, errorMessagePrefix);
        return ReturnToIndexWithError($"{errorMessagePrefix}: {ex.Message}");
    }

    // Protected method for setting up successful file upload response
    protected IActionResult SetupSuccessfulUploadResponse(string savedFileName, IEnumerable<dynamic> records, int recordsToShow = 10)
    {
        ViewBag.Message = "CSV file uploaded and saved successfully!";
        ViewBag.Records = records.Take(recordsToShow).ToList();
        ViewBag.FilePath = $"/files/{savedFileName}";
        var filesList = _csvFileService.GetCsvFileNames();
        return View("Index", filesList);
    }

    // Protected method for getting files list and returning to Index view with error
    protected IActionResult ReturnToIndexWithError(string errorMessage)
    {
        ViewBag.ErrorMessage = errorMessage;
        var files = _csvFileService.GetCsvFileNames();
        return View("Index", files);
    }

    // Protected method for getting files list and returning to Index view
    protected IActionResult ReturnToIndexWithFiles(object? model = null)
    {
        var files = _csvFileService.GetCsvFileNames();
        return View("Index", model ?? files);
    }

    // Chart-related helper methods
    protected async Task<Models.Chart.ChartConfigurationViewModel> BuildConfigurationViewModel(ChartConfiguration configuration, string dataSource)
    {
        if (_dataService == null || _chartService == null)
            throw new InvalidOperationException("Chart services not available in this controller");

        var availableColumns = await _dataService.GetColumnsAsync(dataSource);
        var columnValues = new Dictionary<string, List<string>>();

        // Get values for categorical columns (limited for performance)
        var categoricalColumns = availableColumns.Where(c => c.IsCategory || !c.IsNumeric).Take(10);
        foreach (var column in categoricalColumns)
        {
            var values = await _dataService.GetColumnValuesAsync(dataSource, column.Column, 100);
            columnValues[column.Column] = values;
        }

        return new Models.Chart.ChartConfigurationViewModel
        {
            Configuration = configuration,
            AvailableColumns = availableColumns,
            ChartTypes = ChartTypes.GetNames(),
            ColorPalettes = ColorPalettes.GetNames(),
            ColumnValues = columnValues,
            DataSource = dataSource,
            IsEditMode = configuration.Id > 0,
            SavedConfigurations = await _chartService.GetConfigurationsAsync(dataSource)
        };
    }

    protected ChartConfiguration CreateDefaultConfiguration(string dataSource)
    {
        var config = new ChartConfiguration
        {
            Name = "New Chart",
            CsvFile = dataSource,
            ChartType = "Column",
            ChartStyle = "2D",
            ChartPalette = "BrightPastel",
            Width = 800,
            Height = 400,
            Title = "Chart Title",
            ShowLegend = true,
            IsAnimated = true
        };

        // Create a default series so the form has something to work with
        config.Series.Add(new ChartSeries
        {
            Name = "Series 1",
            DataColumn = "", // Will be selected by user
            AggregationFunction = "Sum",
            IsVisible = true,
            DisplayOrder = 1
        });

        // Create default X-axis
        config.XAxis = new ChartAxis
        {
            AxisType = "X",
            DataColumn = "", // Will be selected by user
            Title = "Category"
        };

        return config;
    }

    protected List<ExportOption> GetExportOptions()
    {
        return new List<ExportOption>
    {
        new ExportOption { Key = "png", Name = "PNG Image", Description = "Portable Network Graphics", MimeType = "image/png", FileExtension = "png" },
        new ExportOption { Key = "jpg", Name = "JPEG Image", Description = "Joint Photographic Experts Group", MimeType = "image/jpeg", FileExtension = "jpg" },
        new ExportOption { Key = "svg", Name = "SVG Vector", Description = "Scalable Vector Graphics", MimeType = "image/svg+xml", FileExtension = "svg" },
        new ExportOption { Key = "pdf", Name = "PDF Document", Description = "Portable Document Format", MimeType = "application/pdf", FileExtension = "pdf" },
        new ExportOption { Key = "csv", Name = "CSV Data", Description = "Comma Separated Values", MimeType = "text/csv", FileExtension = "csv" },
        new ExportOption { Key = "json", Name = "JSON Data", Description = "JavaScript Object Notation", MimeType = "application/json", FileExtension = "json" }
    };
    }

    // Protected method for handling chart errors with logging
    protected IActionResult HandleChartException(Exception ex, string errorMessagePrefix = "An error occurred while processing the chart", string? viewName = null)
    {
        _logger.LogError(ex, errorMessagePrefix);

        var viewModel = new ChartIndexViewModel
        {
            ErrorMessage = $"{errorMessagePrefix}: {ex.Message}",
            AvailableDataSources = new List<string>(),
            SavedConfigurations = new List<ChartConfigurationSummary>()
        };

        if (!string.IsNullOrEmpty(viewName))
        {
            return View(viewName, viewModel);
        }

        return View(viewModel);
    }

    // Protected method for handling chart configuration errors
    protected IActionResult HandleChartConfigurationError(string errorMessage, ChartConfiguration? configuration = null, string? dataSource = null)
    {
        if (configuration != null && !string.IsNullOrEmpty(dataSource))
        {
            var viewModel = new Models.Chart.ChartConfigurationViewModel
            {
                Configuration = configuration,
                ErrorMessage = errorMessage,
                DataSource = dataSource
            };
            return View("Configure", viewModel);
        }

        return View("Configure", new Models.Chart.ChartConfigurationViewModel
        {
            ErrorMessage = errorMessage
        });
    }

    // Pivot Table helper methods
    protected List<Dictionary<string, object>> ConvertCsvToJsonData(
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

    protected IActionResult ExportAsCsv(List<Dictionary<string, object>> data, string fileName)
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

    protected IActionResult ExportAsTsv(List<Dictionary<string, object>> data, string fileName)
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

    protected IActionResult ExportAsJson<T>(List<Dictionary<string, object>> data, T config, string fileName)
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

    protected IActionResult ExportAsExcel(List<Dictionary<string, object>> data, string fileName)
    {
        // For now, export as CSV since we don't have Excel library
        // In a real application, you'd use libraries like EPPlus or ClosedXML
        return ExportAsCsv(data, fileName.Replace(".xlsx", ".csv"));
    }

    protected PivotTableViewModel BuildPivotTableViewModel(string? currentFile = null)
    {
        var model = new PivotTableViewModel
        {
            AvailableFiles = _csvFileService.GetCsvFileNames(),
            CurrentFile = currentFile ?? HttpContext.Session.GetString("CurrentCsvFile") ?? string.Empty
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

        return model;
    }

    protected LoadCsvDataResponse ProcessLoadCsvDataRequest(LoadCsvDataRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FileName))
            {
                return new LoadCsvDataResponse
                {
                    Success = false,
                    Error = "File name is required"
                };
            }

            var records = _csvFileService.ReadCsvRecords(request.FileName);
            if (!records.Any())
            {
                return new LoadCsvDataResponse
                {
                    Success = false,
                    Error = "File not found or contains no data"
                };
            }

            var processedData = ConvertCsvToJsonData(records, request.SelectedColumns);
            var firstRecord = (IDictionary<string, object>)records.First();
            var columns = request.SelectedColumns?.Any() == true
                ? request.SelectedColumns
                : firstRecord.Keys.ToList();

            HttpContext.Session.SetString("CurrentCsvFile", request.FileName);

            return new LoadCsvDataResponse
            {
                Success = true,
                Data = processedData,
                Columns = columns,
                RecordCount = processedData.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading CSV data for file: {FileName}", request?.FileName);
            return new LoadCsvDataResponse
            {
                Success = false,
                Error = "Failed to load CSV data: " + ex.Message
            };
        }
    }
}
