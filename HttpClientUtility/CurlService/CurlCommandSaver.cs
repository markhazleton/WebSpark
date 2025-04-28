using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace HttpClientUtility.CurlService;

/// <summary>
/// Provides functionality to generate and save cURL commands from HTTP requests for debugging and logging purposes.
/// </summary>
public class CurlCommandSaver
{
    // SemaphoreSlim is used to enforce exclusive access during file writes.
    private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    private readonly string _csvFilePath;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurlCommandSaver"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging operations</param>
    /// <param name="configuration">The configuration containing settings for the command saver</param>
    /// <exception cref="ArgumentException">Thrown when the CsvOutputFolder configuration setting is not properly configured</exception>
    public CurlCommandSaver(ILogger logger, IConfiguration configuration)
    {
        _logger = logger;

        // Retrieve the CSV output folder setting from _configuration.
        string? csvOutputFolder = configuration["CsvOutputFolder"];
        if (string.IsNullOrWhiteSpace(csvOutputFolder))
        {
            throw new ArgumentException("CsvOutputFolder is not configured properly.");
        }

        // Ensure the output directory exists.
        Directory.CreateDirectory(csvOutputFolder);

        // Build the full path for the CSV file.
        _csvFilePath = Path.Combine(csvOutputFolder, "curl_commands.csv");
    }

    /// <summary>
    /// Saves an HTTP request as a cURL command to a CSV file.
    /// </summary>
    /// <param name="request">The HTTP request message to convert and save</param>
    /// <param name="memberName">Name of the calling member (automatically populated)</param>
    /// <param name="filePath">File path of the calling code (automatically populated)</param>
    /// <param name="lineNumber">Line number of the calling code (automatically populated)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SaveCurlCommandAsync(HttpRequestMessage request,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        // Build the curl command string.
        var curlCommand = new StringBuilder();
        curlCommand.Append("curl");

        // Include the HTTP method if it's not GET (curl defaults to GET).
        if (request.Method != HttpMethod.Get)
        {
            curlCommand.Append(" -X ").Append(request.Method.Method);
        }

        // Include request content as a data parameter, if available.
        if (request.Content != null)
        {
            string content = await request.Content.ReadAsStringAsync().ConfigureAwait(true);
            curlCommand.Append(" -d '").Append(content.Replace("'", "\\'")).Append('\'');
        }

        // Append the request URL.
        curlCommand.Append(" ").Append(request.RequestUri);

        _logger.LogInformation("Created curl command: {Command}", curlCommand.ToString());

        // Create a record with the current timestamp and the generated curl command.
        var record = new CurlCommandRecord
        {
            CurlCommand = curlCommand.ToString(),
            RequestPath = request.RequestUri?.ToString() ?? string.Empty,
            RequestMethod = request.Method.Method,
            CallingMethod = memberName,
            CallingFile = filePath,
            CallingLineNumber = lineNumber
        };

        // Write the record to the CSV file using a file lock for thread safety.
        await _fileLock.WaitAsync().ConfigureAwait(true);
        try
        {
            bool fileExists = File.Exists(_csvFilePath);

            using (var stream = File.Open(_csvFilePath, fileExists ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // If the file is new, write the header first.
                if (!fileExists)
                {
                    csv.WriteHeader<CurlCommandRecord>();
                    await csv.NextRecordAsync().ConfigureAwait(true);
                }

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(true);
            }

            _logger.LogInformation("Saved curl command record to CSV file at {Path}", _csvFilePath);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Represents a record of a cURL command with metadata about where it was generated.
    /// </summary>
    private class CurlCommandRecord
    {
        /// <summary>
        /// Gets or sets the file path where the request was initiated.
        /// </summary>
        public string CallingFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the line number in the file where the request was initiated.
        /// </summary>
        public int CallingLineNumber { get; set; }

        /// <summary>
        /// Gets or sets the method name where the request was initiated.
        /// </summary>
        public string CallingMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the generated cURL command string.
        /// </summary>
        public string CurlCommand { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP method used in the request.
        /// </summary>
        public string RequestMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL or path of the request.
        /// </summary>
        public string RequestPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the record was created.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
