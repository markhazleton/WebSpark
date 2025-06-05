using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;
using WebSpark.HttpClientUtility.MemoryCache;

namespace DataSpark.Web.Controllers;

public class UnivariateController : DataSparkBaseController<UnivariateController>
{
    private readonly CsvProcessingService _csvProcessingService;

    public UnivariateController(
        CsvProcessingService csvProcessingService,
        IMemoryCacheManager memoryCacheManager,
        IConfiguration configuration,
        ILogger<UnivariateController> logger,
        CsvFileService csvFileService)
        : base(memoryCacheManager, configuration, logger, csvFileService)
    {
        _csvProcessingService = csvProcessingService;
    }

    [HttpGet]
    public IActionResult Index(string? fileName)
    {
        var files = CsvFileService.GetCsvFileNames();
        if (string.IsNullOrEmpty(fileName) && files.Any())
            fileName = files.First();

        if (string.IsNullOrEmpty(fileName))
        {
            ViewBag.ErrorMessage = "No CSV files found. Please upload a file first.";
            ViewBag.Files = files;
            return View(new CsvViewModel { FileName = null });
        }

        // Use CsvProcessingService to generate a full univariate analysis
        var model = _csvProcessingService.ProcessCsvWithFallbackAsync(fileName).GetAwaiter().GetResult();
        model.FileName = fileName;
        ViewBag.Files = files;

        // Enhanced column type detection and analysis is now handled in UnivariateAnalysisExtensions
        // No need to re-read CSV data here - leverage the existing sophisticated analysis
        foreach (var col in model.ColumnDetails)
        {
            // Set IsNumeric and IsCategory flags based on the enhanced analysis
            col.IsNumeric = IsColumnNumericType(col);
            col.IsCategory = IsColumnCategoricalType(col);

            // Update the Type field for UI display
            col.Type = col.IsNumeric ? "Numeric" : "Categorical";
        }

        return View(model);
    }
    [HttpPost]
    public IActionResult Analyze(string fileName, string columnName)
    {
        string html = "<div class='alert alert-danger'>Analysis failed</div>";
        try
        {
            fileName = System.Net.WebUtility.HtmlDecode(fileName);
            columnName = System.Net.WebUtility.HtmlDecode(columnName);

            if (!CsvFileService.FileExists(fileName) || string.IsNullOrEmpty(columnName))
            {
                html = $"<div class='alert alert-danger'>{fileName} not found in output folder</div>";
                return Content(html, "text/html");
            }            // Get the existing analysis (uses existing service pattern)
            var cachedModel = _csvProcessingService.ProcessCsvWithFallbackAsync(fileName).GetAwaiter().GetResult();

            // Find the column in the cached analysis
            var columnInfo = cachedModel.ColumnDetails.FirstOrDefault(c => c.Column != null && c.Column.Equals(columnName, StringComparison.OrdinalIgnoreCase));
            if (columnInfo == null)
            {
                html = $"<div class='alert alert-danger'>Column '{columnName}' does not exist in the analysis</div>";
                return Content(html, "text/html");
            }

            // Generate comprehensive analysis HTML using the enhanced column info
            html = GenerateDetailedAnalysisHtml(columnInfo, fileName);

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error analyzing column {ColumnName} in file {FileName}", columnName, fileName);
            html = $"<div class='alert alert-danger'>Analysis error: {ex.Message}</div>";
            return Content(html, "text/html");
        }
    }

    /// <summary>
    /// Generates detailed analysis HTML for a column using the enhanced analysis data
    /// </summary>
    private string GenerateDetailedAnalysisHtml(ColumnInfo columnInfo, string fileName)
    {
        var html = new StringBuilder();

        try
        {
            // Header with column information
            html.AppendLine($"<div class='card mt-3'>");
            html.AppendLine($"<div class='card-header'>");
            html.AppendLine($"<h4>📊 Detailed Analysis: {columnInfo.Column}</h4>");
            html.AppendLine($"<span class='badge badge-{(columnInfo.IsNumeric ? "primary" : "secondary")}'>{columnInfo.Type}</span>");
            html.AppendLine($"</div>");
            html.AppendLine($"<div class='card-body'>");

            // Basic statistics section
            html.AppendLine($"<div class='row'>");
            html.AppendLine($"<div class='col-md-6'>");
            html.AppendLine($"<h5>📈 Basic Statistics</h5>");
            html.AppendLine($"<ul class='list-group list-group-flush'>");
            html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Total Records:</span><strong>{columnInfo.NonNullCount + columnInfo.NullCount:N0}</strong></li>");
            html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Non-null Values:</span><strong>{columnInfo.NonNullCount:N0}</strong></li>");
            html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Missing Values:</span><strong>{columnInfo.NullCount:N0}</strong></li>");
            html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Unique Values:</span><strong>{columnInfo.UniqueCount:N0}</strong></li>");

            if (columnInfo.Mode != null)
                html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Mode:</span><strong>{columnInfo.Mode} ({columnInfo.ModeFrequency:N0}x)</strong></li>");

            html.AppendLine($"</ul>");
            html.AppendLine($"</div>");

            // Advanced statistics for numeric columns
            if (columnInfo.IsNumeric && !double.IsNaN(columnInfo.Mean))
            {
                html.AppendLine($"<div class='col-md-6'>");
                html.AppendLine($"<h5>🔢 Numeric Statistics</h5>");
                html.AppendLine($"<ul class='list-group list-group-flush'>");
                html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Mean:</span><strong>{columnInfo.Mean:F2}</strong></li>");

                if (columnInfo.Median.HasValue)
                    html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Median:</span><strong>{columnInfo.Median.Value:F2}</strong></li>");

                html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Std Dev:</span><strong>{columnInfo.StandardDeviation:F2}</strong></li>");
                html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Min:</span><strong>{columnInfo.Min}</strong></li>");
                html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Max:</span><strong>{columnInfo.Max}</strong></li>");

                if (columnInfo.Q1.HasValue && columnInfo.Q3.HasValue)
                {
                    html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Q1 (25%):</span><strong>{columnInfo.Q1.Value:F2}</strong></li>");
                    html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Q3 (75%):</span><strong>{columnInfo.Q3.Value:F2}</strong></li>");
                }

                html.AppendLine($"<li class='list-group-item d-flex justify-content-between'><span>Skewness:</span><strong>{columnInfo.Skewness:F3}</strong></li>");
                html.AppendLine($"</ul>");
                html.AppendLine($"</div>");
            }

            html.AppendLine($"</div>");            // Observations section with enhanced styling
            if (columnInfo.Observations.Any())
            {
                html.AppendLine($"<div class='mt-4'>");
                html.AppendLine($"<h5>🔍 Key Observations</h5>");
                html.AppendLine($"<div class='row'>");

                var observations = columnInfo.Observations.Take(10).ToList(); // Limit to top 10 observations
                var itemsPerColumn = Math.Max(1, (observations.Count + 1) / 2); // Split into 2 columns

                for (int col = 0; col < 2; col++)
                {
                    var startIndex = col * itemsPerColumn;
                    var endIndex = Math.Min(startIndex + itemsPerColumn, observations.Count);

                    if (startIndex < observations.Count)
                    {
                        html.AppendLine($"<div class='col-md-6'>");

                        for (int i = startIndex; i < endIndex; i++)
                        {
                            var observation = observations[i];
                            var icon = GetObservationIcon(observation);
                            var badgeColor = GetObservationBadgeColor(observation);

                            html.AppendLine($"<div class='card mb-2 border-left-{badgeColor}' style='border-left: 4px solid;'>");
                            html.AppendLine($"<div class='card-body py-2 px-3'>");
                            html.AppendLine($"<div class='d-flex align-items-start'>");
                            html.AppendLine($"<span class='me-2' style='font-size: 1.1em;'>{icon}</span>");
                            html.AppendLine($"<span class='text-muted small'>{observation}</span>");
                            html.AppendLine($"</div>");
                            html.AppendLine($"</div>");
                            html.AppendLine($"</div>");
                        }

                        html.AppendLine($"</div>");
                    }
                }

                html.AppendLine($"</div>");
                html.AppendLine($"</div>");
            }

            // Try to generate visualization using existing service
            try
            {
                // Read the data for visualization (only when needed)
                var records = CsvFileService.ReadCsvRecords(fileName);
                if (records.Any())
                {
                    var dataTable = ConvertToDataTable(records);
                    if (dataTable.Columns.Contains(columnInfo.Column ?? string.Empty))
                    {
                        string svg = _csvProcessingService.GetScottPlotSvg(columnInfo.Column ?? string.Empty, dataTable);
                        html.AppendLine($"<div class='mt-4'>");
                        html.AppendLine($"<h5>📊 Visualization</h5>");
                        html.AppendLine($"<div class='text-center'>{svg}</div>");
                        html.AppendLine($"</div>");
                    }
                }
            }
            catch (Exception vizEx)
            {
                Logger.LogWarning(vizEx, "Could not generate visualization for column {ColumnName}", columnInfo.Column);
                html.AppendLine($"<div class='mt-4'>");
                html.AppendLine($"<div class='alert alert-warning'>Visualization temporarily unavailable</div>");
                html.AppendLine($"</div>");
            }

            html.AppendLine($"</div>");
            html.AppendLine($"</div>");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating detailed analysis HTML for column {ColumnName}", columnInfo.Column);
            return $"<div class='alert alert-danger'>Error generating detailed analysis: {ex.Message}</div>";
        }

        return html.ToString();
    }

    [HttpGet]
    public IActionResult FullAnalysisReport()
    {
        var files = CsvFileService.GetCsvFileNames();
        return View(files);
    }

    [HttpGet]
    public IActionResult CompleteAnalysis()
    {
        var files = CsvFileService.GetCsvFileNames();
        return View(files);
    }

    private static DataTable ConvertToDataTable(List<dynamic> records)
    {
        var dt = new DataTable();

        if (records.Any())
        {
            var first = records[0] as IDictionary<string, object>;
            if (first != null)
            {
                // Add columns
                foreach (var key in first.Keys)
                {
                    dt.Columns.Add(key);
                }

                // Add rows
                foreach (var record in records)
                {
                    var dict = record as IDictionary<string, object>;
                    if (dict != null)
                    {
                        var row = dt.NewRow();
                        foreach (var key in dict.Keys)
                        {
                            row[key] = dict[key]?.ToString() ?? string.Empty;
                        }
                        dt.Rows.Add(row);
                    }
                }
            }
        }

        return dt;
    }

    /// <summary>
    /// Enhanced logic to determine if a column should be treated as numeric for analysis
    /// </summary>
    private static bool IsColumnNumericType(ColumnInfo columnInfo)
    {
        // Check if we have numeric values calculated
        if (double.IsNaN(columnInfo.Mean) || columnInfo.Min == null || columnInfo.Max == null)
            return false;

        // If we have a good sample of numeric values and sufficient unique values
        // relative to the total count, treat as numeric
        double uniqueRatio = (double)columnInfo.UniqueCount / Math.Max(columnInfo.NonNullCount, 1);

        // Consider numeric if:
        // 1. We have actual numeric statistics
        // 2. Either high unique ratio (>0.1) OR more than 10 unique values
        // 3. Not all values are unique (which might indicate IDs)
        return uniqueRatio > 0.1 ||
               (columnInfo.UniqueCount > 10 && columnInfo.UniqueCount < columnInfo.NonNullCount);
    }

    /// <summary>
    /// Enhanced logic to determine if a column should be treated as categorical
    /// </summary>
    private static bool IsColumnCategoricalType(ColumnInfo columnInfo)
    {
        // Inverse of numeric logic, but also handle special cases
        if (!IsColumnNumericType(columnInfo))
            return true;

        // Even if numeric, treat as categorical if very few unique values
        double uniqueRatio = (double)columnInfo.UniqueCount / Math.Max(columnInfo.NonNullCount, 1);
        return uniqueRatio <= 0.05 || columnInfo.UniqueCount <= 5;
    }

    /// <summary>
    /// Gets an appropriate icon for an observation based on its content
    /// </summary>
    private string GetObservationIcon(string observation)
    {
        var lowerObs = observation.ToLower();

        if (lowerObs.Contains("missing") || lowerObs.Contains("null") || lowerObs.Contains("empty"))
            return "⚠️";
        if (lowerObs.Contains("outlier") || lowerObs.Contains("extreme"))
            return "📊";
        if (lowerObs.Contains("skew") || lowerObs.Contains("distribution"))
            return "📈";
        if (lowerObs.Contains("correlation") || lowerObs.Contains("relationship"))
            return "🔗";
        if (lowerObs.Contains("pattern") || lowerObs.Contains("trend"))
            return "📋";
        if (lowerObs.Contains("unique") || lowerObs.Contains("distinct"))
            return "🔢";
        if (lowerObs.Contains("frequency") || lowerObs.Contains("common"))
            return "📊";
        if (lowerObs.Contains("range") || lowerObs.Contains("min") || lowerObs.Contains("max"))
            return "📏";
        if (lowerObs.Contains("average") || lowerObs.Contains("mean") || lowerObs.Contains("median"))
            return "📊";

        return "💡"; // Default icon
    }

    /// <summary>
    /// Gets an appropriate Bootstrap color class for an observation badge
    /// </summary>
    private string GetObservationBadgeColor(string observation)
    {
        var lowerObs = observation.ToLower();

        if (lowerObs.Contains("missing") || lowerObs.Contains("null") || lowerObs.Contains("empty") || lowerObs.Contains("error"))
            return "warning";
        if (lowerObs.Contains("outlier") || lowerObs.Contains("extreme") || lowerObs.Contains("anomal"))
            return "danger";
        if (lowerObs.Contains("normal") || lowerObs.Contains("good") || lowerObs.Contains("valid"))
            return "success";
        if (lowerObs.Contains("skew") || lowerObs.Contains("unusual") || lowerObs.Contains("irregular"))
            return "info";

        return "primary"; // Default color
    }
}
