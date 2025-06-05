using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.Analysis;
using System.Globalization;
using System.Text;

namespace DataSpark.Web.Services;

/// <summary>
/// Service for common CSV file operations to eliminate duplicate code across controllers
/// </summary>
public class CsvFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CsvFileService> _logger;

    public CsvFileService(IWebHostEnvironment env, IConfiguration configuration, ILogger<CsvFileService> logger)
    {
        _env = env;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get the configured CSV files directory path
    /// </summary>
    public string GetCsvFilesPath()
    {
        var outputFolder = _configuration["CsvOutputFolder"];
        return !string.IsNullOrEmpty(outputFolder)
            ? outputFolder
            : Path.Combine(_env.WebRootPath, "files");
    }

    /// <summary>
    /// List all CSV files in the configured directory
    /// </summary>
    public List<string> GetCsvFileNames()
    {
        var filesPath = GetCsvFilesPath();
        if (!Directory.Exists(filesPath))
        {
            _logger.LogWarning("CSV files directory does not exist: {Path}", filesPath);
            return new List<string>();
        }

        return Directory.GetFiles(filesPath, "*.csv")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Select(f => f!)
            .ToList();
    }

    /// <summary>
    /// Get full file path for a CSV file
    /// </summary>
    public string? GetFilePath(string fileName)
    {
        if (!IsValidFileName(fileName))
            return null;

        var filesPath = GetCsvFilesPath();
        if (!Directory.Exists(filesPath))
            return null;

        var filePath = Path.Combine(filesPath, fileName);
        return File.Exists(filePath) ? filePath : null;
    }

    /// <summary>
    /// Check if a CSV file exists
    /// </summary>
    public bool FileExists(string fileName)
    {
        return GetFilePath(fileName) != null;
    }

    // CSV Injection sanitization for Excel
    private string SanitizeForExcel(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.StartsWith("=") || value.StartsWith("+") || value.StartsWith("-") || value.StartsWith("@"))
            return "'" + value;
        return value;
    }

    // Async method to read CSV headers (only async version)
    public async Task<CsvOperationResult<string>> GetCsvHeadersAsync(string fileName, char delimiter = ',', Encoding? encoding = null)
    {
        var result = new CsvOperationResult<string>();
        if (!IsValidFileName(fileName))
        {
            result.ErrorMessage = "Invalid file name.";
            return result;
        }
        var filePath = GetFilePath(fileName);
        if (filePath == null)
        {
            result.ErrorMessage = "File not found.";
            return result;
        }
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter.ToString() });
            if (await csv.ReadAsync())
            {
                csv.ReadHeader();
                result.Data = csv.HeaderRecord?.ToList() ?? new List<string>();
                result.Success = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading headers from CSV file: {FileName}", fileName);
            result.ErrorMessage = ex.Message;
        }
        return result;
    }

    // Async method to read CSV records as dynamic (only async version)
    public async Task<CsvOperationResult<dynamic>> ReadCsvRecordsAsync(string fileName, char delimiter = ',', Encoding? encoding = null)
    {
        var result = new CsvOperationResult<dynamic>();
        if (!IsValidFileName(fileName))
        {
            result.ErrorMessage = "Invalid file name.";
            return result;
        }
        var filePath = GetFilePath(fileName);
        if (filePath == null)
        {
            result.ErrorMessage = "File not found.";
            return result;
        }
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter.ToString() });
            await foreach (var record in csv.GetRecordsAsync<dynamic>())
            {
                result.Data.Add(record);
            }
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading CSV records from file: {FileName}", fileName);
            result.ErrorMessage = ex.Message;
        }
        return result;
    }

    // Async method to read CSV records as strongly-typed (only async version)
    public async Task<CsvOperationResult<T>> ReadCsvRecordsAsync<T>(string fileName, char delimiter = ',', Encoding? encoding = null)
    {
        var result = new CsvOperationResult<T>();
        if (!IsValidFileName(fileName))
        {
            result.ErrorMessage = "Invalid file name.";
            return result;
        }
        var filePath = GetFilePath(fileName);
        if (filePath == null)
        {
            result.ErrorMessage = "File not found.";
            return result;
        }
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter.ToString() });
            await foreach (var record in csv.GetRecordsAsync<T>())
            {
                result.Data.Add(record);
            }
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading CSV records from file: {FileName}", fileName);
            result.ErrorMessage = ex.Message;
        }
        return result;
    }

    // Async method to get DataFrame (for analytics)
    public async Task<CsvOperationResult<DataFrame>> ReadCsvAsDataFrameAsync(string fileName, char delimiter = ',', Encoding? encoding = null, bool allString = false)
    {
        var result = new CsvOperationResult<DataFrame>();
        if (!IsValidFileName(fileName))
        {
            result.ErrorMessage = "Invalid file name.";
            return result;
        }
        var filePath = GetFilePath(fileName);
        if (filePath == null)
        {
            result.ErrorMessage = "File not found.";
            return result;
        }
        try
        {
            var df = await Task.Run(() =>
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (allString)
                {
                    int colCount = CsvProcessingUtils.GetColumnCount(filePath);
                    return DataFrame.LoadCsv(
                        stream,
                        separator: delimiter,
                        header: true,
                        dataTypes: Enumerable.Repeat(typeof(string), colCount).ToArray(),
                        encoding: encoding ?? Encoding.UTF8,
                        cultureInfo: CultureInfo.InvariantCulture
                    );
                }
                else
                {
                    try
                    {
                        return DataFrame.LoadCsv(
                            stream,
                            separator: delimiter,
                            header: true,
                            encoding: encoding ?? Encoding.UTF8,
                            cultureInfo: CultureInfo.InvariantCulture
                        );
                    }
                    catch (FormatException)
                    {
                        // Retry with all columns as string if type inference fails
                        stream.Position = 0;
                        int colCount = CsvProcessingUtils.GetColumnCount(filePath);
                        return DataFrame.LoadCsv(
                            stream,
                            separator: delimiter,
                            header: true,
                            dataTypes: Enumerable.Repeat(typeof(string), colCount).ToArray(),
                            encoding: encoding ?? Encoding.UTF8,
                            cultureInfo: CultureInfo.InvariantCulture
                        );
                    }
                }
            });
            result.Data.Add(df);
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading DataFrame from CSV file: {FileName}", fileName);
            result.ErrorMessage = ex.Message;
            // Fault tolerance: return an empty DataFrame if loading fails
            result.Data.Add(new DataFrame());
            result.Success = false;
        }
        return result;
    }

    // Visualization with optional Excel sanitization
    public async Task<CsvDataResult> ReadCsvForVisualizationAsync(string fileName, bool sanitizeForExcel = false)
    {
        var recordsResult = await ReadCsvRecordsAsync<dynamic>(fileName);
        if (!recordsResult.Success || !recordsResult.Data.Any())
        {
            return new CsvDataResult
            {
                Headers = new List<string>(),
                Columns = new Dictionary<string, List<string>>(),
                Records = new List<dynamic>()
            };
        }
        var records = recordsResult.Data;
        var headers = ((IDictionary<string, object>)records[0]).Keys.ToList();
        var columns = new Dictionary<string, List<string>>();
        foreach (var header in headers)
        {
            columns[header] = records.Select(r =>
                sanitizeForExcel ? SanitizeForExcel(((IDictionary<string, object>)r)[header]?.ToString() ?? string.Empty)
                                 : ((IDictionary<string, object>)r)[header]?.ToString() ?? string.Empty
            ).ToList();
        }
        return new CsvDataResult
        {
            Headers = headers,
            Columns = columns,
            Records = records
        };
    }

    // Legacy sync method for compatibility (calls async version)
    public CsvDataResult ReadCsvForVisualization(string fileName, bool sanitizeForExcel = false)
    {
        // Call async version and block (for legacy controller compatibility)
        return ReadCsvForVisualizationAsync(fileName, sanitizeForExcel).GetAwaiter().GetResult();
    }

    // Legacy sync method for compatibility (calls async version)
    public List<dynamic> ReadCsvRecords(string fileName)
    {
        var result = ReadCsvRecordsAsync(fileName).GetAwaiter().GetResult();
        return result.Success ? result.Data : new List<dynamic>();
    }

    // Legacy sync method for compatibility (calls async version)
    public List<T> ReadCsvRecords<T>(string fileName)
    {
        var result = ReadCsvRecordsAsync<T>(fileName).GetAwaiter().GetResult();
        return result.Success ? result.Data : new List<T>();
    }

    /// <summary>
    /// Save uploaded file to the CSV directory
    /// </summary>
    public async Task<string?> SaveUploadedFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        var filesPath = GetCsvFilesPath();
        if (!Directory.Exists(filesPath))
        {
            try
            {
                Directory.CreateDirectory(filesPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create CSV files directory: {Path}", filesPath);
                return null;
            }
        }

        var originalFileName = Path.GetFileName(file.FileName);
        if (!IsValidFileName(originalFileName))
            return null;

        var fileName = originalFileName;
        var filePath = Path.Combine(filesPath, fileName);

        // If file exists, append timestamp to avoid overwrite
        if (File.Exists(filePath))
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            fileName = $"{name}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
            filePath = Path.Combine(filesPath, fileName);
        }

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            _logger.LogInformation("File saved successfully: {FilePath}", filePath);
            // Return only the file name, not the full path
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {FileName}", fileName);
            return null;
        }
    }

    // Helper: Validate file name for security (no path traversal, no invalid chars)
    private bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;
        if (fileName.Contains("..") || fileName.Contains(Path.DirectorySeparatorChar) || fileName.Contains(Path.AltDirectorySeparatorChar)) return false;
        return true;
    }
}

/// <summary>
/// Result structure for CSV data reading
/// </summary>
public class CsvDataResult
{
    public List<string> Headers { get; set; } = new();
    public Dictionary<string, List<string>> Columns { get; set; } = new();
    public List<dynamic> Records { get; set; } = new();
}

public class CsvOperationResult<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
