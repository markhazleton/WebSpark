using HttpClientUtility.MemoryCache;
using System.Collections.Concurrent;
using System.Diagnostics;
using WebSpark.Portal.Areas.DataSpark.Models;

namespace WebSpark.Portal.Areas.DataSpark.Controllers;

[Area("DataSpark")]
public abstract class DataSparkBaseController<T> : Controller where T : DataSparkBaseController<T>
{
    protected string? OutputFolder { get; }
    protected readonly IConfiguration Configuration;
    protected readonly ILogger<T> Logger;
    private readonly IMemoryCacheManager _cacheManager;
    private const string DataSparkCacheKey = "DataSpark";

    protected DataSparkBaseController(
        IMemoryCacheManager memoryCacheManager,
        IConfiguration configuration,
        ILogger<T> logger)
    {
        Configuration = configuration;
        Logger = logger;
        _cacheManager = memoryCacheManager;
        OutputFolder = configuration["CsvOutputFolder"];
    }
    public ConcurrentDictionary<string, CsvViewModel?> GetDataSpark()
    {
        return _cacheManager.Get(DataSparkCacheKey, () =>
        {
            var dataSpark = LoadCsvFiles();
            return dataSpark;
        }, 30); // Cache time in minutes
    }

    private ConcurrentDictionary<string, CsvViewModel?> LoadCsvFiles()
    {
        var csvFiles = new ConcurrentDictionary<string, CsvViewModel?>();
        if (!string.IsNullOrEmpty(OutputFolder) && Directory.Exists(OutputFolder))
        {
            Directory.GetFiles(OutputFolder, "*.csv")
                     .ToList()
                     .ForEach(file => csvFiles[Path.GetFileName(file)!] = null);
        }
        return csvFiles;
    }

    public List<string> GetCsvFiles()
    {
        return [.. GetDataSpark().Keys]; // Return the keys of the dictionary
    }

    public CsvViewModel? GetViewModelForFile(string fileName)
    {
        GetDataSpark().TryGetValue(fileName, out var viewModel);
        return viewModel;
    }

    public bool TryUpdateCsvFile(string fileName, CsvViewModel viewModel)
    {
        // Updates or adds the CsvViewModel in the dictionary
        GetDataSpark().AddOrUpdate(fileName, viewModel, (key, oldValue) => viewModel);
        return true;
    }

    protected async Task<string?> AddFileAsync(IFormFile file)
    {
        if (OutputFolder == null || !EnsureDirectoryExists(OutputFolder))
        {
            Logger.LogError("Output folder is not set or does not exist.");
            return null;
        }

        var fileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(OutputFolder, fileName);

        try
        {
            // Save the file to the specified path
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Refresh the cached list of CSV files in a thread-safe manner
            RefreshCsvFiles();

            return filePath;
        }
        catch (Exception ex)
        {
            LogError($"Failed to save file {fileName}.", ex);
            return null;
        }
    }

    protected void RefreshCsvFiles()
    {
        var refreshedFiles = LoadCsvFiles();
        foreach (var file in refreshedFiles)
        {
            GetDataSpark().AddOrUpdate(file.Key, file.Value, (key, oldValue) => file.Value); // Thread-safe update
        }
    }

    protected string? GetFilePath(string fileName)
    {
        if (string.IsNullOrEmpty(OutputFolder) || string.IsNullOrEmpty(fileName) || !EnsureDirectoryExists(OutputFolder))
        {
            return null;
        }

        var files = Directory.GetFiles(OutputFolder, "*", SearchOption.TopDirectoryOnly)
                             .Select(f => new FileInfo(f))
                             .ToList();

        var fileInfo = files.FirstOrDefault(f => string.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase));
        return fileInfo?.FullName;
    }

    private bool EnsureDirectoryExists(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Failed to create directory {directoryPath}.", ex);
            return false;
        }
    }

    protected void LogInformation(string message)
    {
        Logger.LogInformation("LogInformation: {Message}", message);
    }

    protected void LogError(string message, Exception ex)
    {
        Logger.LogError(ex, "LogError: {Message}", message);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
