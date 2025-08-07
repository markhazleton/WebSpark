using DataSpark.Web.Models;
using DataSpark.Web.Models.Chart;

namespace DataSpark.Web.Services.Chart;

/// <summary>
/// Service for chart configuration management
/// </summary>
public interface IChartService
{
    Task<ChartConfiguration?> GetConfigurationAsync(int id);
    Task<ChartConfiguration> SaveConfigurationAsync(ChartConfiguration config);
    Task<bool> DeleteConfigurationAsync(int id);
    Task<List<ChartConfigurationSummary>> GetConfigurationsAsync(string? dataSource = null);
    Task<ChartConfiguration?> GetConfigurationByNameAsync(string name, string dataSource);
    Task<bool> ConfigurationExistsAsync(string name, string dataSource, int? excludeId = null);
    Task<ChartConfiguration> DuplicateConfigurationAsync(int id, string newName);
    Task<List<ChartConfiguration>> GetConfigurationsByIdsAsync(List<int> ids);
    Task<int> DeleteConfigurationsAsync(List<int> ids);
}

/// <summary>
/// Service for data processing and analysis
/// </summary>
public interface IDataService
{
    Task<ProcessedChartData> ProcessDataAsync(string dataSource, ChartConfiguration config);
    Task<List<ColumnInfo>> GetColumnsAsync(string dataSource);
    Task<List<string>> GetColumnValuesAsync(string dataSource, string column, int maxValues = 1000);
    Task<bool> ValidateDataSourceAsync(string dataSource);
    Task<List<string>> GetAvailableDataSourcesAsync();
    Task<Dictionary<string, List<string>>> GetMultipleColumnValuesAsync(string dataSource, List<string> columns, int maxValues = 100);
    Task<DataSummary> GetDataSummaryAsync(string dataSource);
}

/// <summary>
/// Service for chart rendering and visualization
/// </summary>
public interface IChartRenderingService
{
    Task<ChartRenderResult> RenderChartAsync(ChartConfiguration config, ProcessedChartData data);
    Task<string> GenerateChartJsonAsync(ChartConfiguration config, ProcessedChartData data);
    Task<string> GenerateChartHtmlAsync(ChartConfiguration config, ProcessedChartData data);
    Task<string> GenerateChartScriptAsync(ChartConfiguration config, ProcessedChartData data);
    Task<byte[]> ExportChartAsync(ChartConfiguration config, ProcessedChartData data, string format);
    Task<string> GenerateEmbedCodeAsync(ChartConfiguration config, string baseUrl);
}

/// <summary>
/// Service for chart validation
/// </summary>
public interface IChartValidationService
{
    Task<ValidationResult> ValidateConfigurationAsync(ChartConfiguration config, string? dataSource = null);
    Task<List<string>> ValidateSeriesAsync(ChartSeries series, List<ColumnInfo> columns);
    Task<List<string>> ValidateAxisAsync(ChartAxis axis, List<ColumnInfo> columns);
    Task<List<string>> ValidateFiltersAsync(List<ChartFilter> filters, List<ColumnInfo> columns);
    Task<bool> IsChartTypeCompatibleAsync(string chartType, List<ColumnInfo> columns);
}

/// <summary>
/// Repository interface for chart configuration persistence
/// </summary>
public interface IChartConfigurationRepository
{
    Task<ChartConfiguration?> GetByIdAsync(int id);
    Task<ChartConfiguration?> GetByNameAsync(string name, string dataSource);
    Task<List<ChartConfiguration>> GetByDataSourceAsync(string dataSource);
    Task<List<ChartConfiguration>> GetAllAsync();
    Task<ChartConfiguration> CreateAsync(ChartConfiguration config);
    Task<ChartConfiguration> UpdateAsync(ChartConfiguration config);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name, string dataSource, int? excludeId = null);
    Task<List<ChartConfiguration>> GetByIdsAsync(List<int> ids);
    Task<int> DeleteByIdsAsync(List<int> ids);
    Task<List<ChartConfigurationSummary>> GetSummariesAsync(string? dataSource = null);
}

/// <summary>
/// Service for template management
/// </summary>
public interface IChartTemplateService
{
    Task<List<ChartTemplate>> GetTemplatesAsync();
    Task<ChartTemplate?> GetTemplateAsync(string name);
    Task<ChartConfiguration> ApplyTemplateAsync(string templateName, Dictionary<string, string> columnMappings);
    Task<List<ChartTemplate>> GetTemplatesByCategoryAsync(string category);
    Task<List<string>> GetTemplateCategoriesAsync();
}

/// <summary>
/// Service for chart export functionality
/// </summary>
public interface IChartExportService
{
    Task<byte[]> ExportToPngAsync(ChartConfiguration config, ProcessedChartData data, int width = 800, int height = 400);
    Task<byte[]> ExportToJpegAsync(ChartConfiguration config, ProcessedChartData data, int width = 800, int height = 400);
    Task<string> ExportToSvgAsync(ChartConfiguration config, ProcessedChartData data);
    Task<byte[]> ExportToPdfAsync(ChartConfiguration config, ProcessedChartData data);
    Task<byte[]> ExportToExcelAsync(ChartConfiguration config, ProcessedChartData data);
    Task<string> ExportToCsvAsync(ProcessedChartData data);
    Task<string> ExportToJsonAsync(ChartConfiguration config, ProcessedChartData data);
    Task<List<ExportOption>> GetExportOptionsAsync();
}

/// <summary>
/// Service for chart sharing and collaboration
/// </summary>
public interface IChartSharingService
{
    Task<string> CreateShareLinkAsync(int configurationId, ChartSharingConfig sharingConfig);
    Task<ChartConfiguration?> GetSharedConfigurationAsync(string shareKey);
    Task<bool> ValidateShareAccessAsync(string shareKey, string? domain = null);
    Task<string> GenerateEmbedCodeAsync(int configurationId, int width = 800, int height = 400);
    Task<bool> RevokeShareAccessAsync(string shareKey);
    Task<List<ChartSharingConfig>> GetSharingConfigsAsync(int configurationId);
}

/// <summary>
/// Service for performance optimization and caching
/// </summary>
public interface IChartCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<ProcessedChartData?> GetProcessedDataAsync(string dataSource, string configHash);
    Task SetProcessedDataAsync(string dataSource, string configHash, ProcessedChartData data);
    string GenerateConfigHash(ChartConfiguration config);
}

/// <summary>
/// Service for audit logging and tracking
/// </summary>
public interface IChartAuditService
{
    Task LogConfigurationChangeAsync(int configurationId, string action, string? details = null, string? userId = null);
    Task LogDataAccessAsync(string dataSource, string action, string? userId = null);
    Task LogExportAsync(int configurationId, string format, string? userId = null);
    Task<List<AuditEntry>> GetAuditTrailAsync(int configurationId);
    Task<List<AuditEntry>> GetUserActivityAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
}

/// <summary>
/// Audit entry for tracking changes
/// </summary>
public class AuditEntry
{
    public int Id { get; set; }
    public int ConfigurationId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
