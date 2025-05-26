using DataSpark.Web.Models;
using DataSpark.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using WebSpark.HttpClientUtility.MemoryCache;

namespace DataSpark.Web.Controllers;

public abstract class DataSparkBaseController<T> : Controller where T : DataSparkBaseController<T>
{
    protected string? OutputFolder { get; }
    protected readonly IConfiguration Configuration;
    protected readonly ILogger<T> Logger;
    protected readonly CsvFileService CsvFileService;
    protected readonly IMemoryCacheManager _cacheManager;
    private const string DataSparkCacheKey = "DataSpark";

    protected DataSparkBaseController(
        IMemoryCacheManager memoryCacheManager,
        IConfiguration configuration,
        ILogger<T> logger,
        CsvFileService csvFileService)
    {
        Configuration = configuration;
        Logger = logger;
        CsvFileService = csvFileService;
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
        // Use the CSV file service for consistent file listing
        return CsvFileService.GetCsvFileNames();
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
        // Use the CSV file service for consistent file handling
        var savedFilePath = await CsvFileService.SaveUploadedFileAsync(file);

        if (savedFilePath != null)
        {
            // Refresh the cached list of CSV files
            RefreshCsvFiles();
        }

        return savedFilePath;
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
        // Use the CSV file service for consistent file path resolution
        return CsvFileService.GetFilePath(fileName);
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
