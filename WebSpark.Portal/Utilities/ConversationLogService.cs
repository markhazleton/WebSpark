using System.Collections.Concurrent;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using System.Threading.Channels;

namespace WebSpark.Portal.Utilities;

public interface IConversationLogService
{
    ValueTask EnqueueAsync(ConversationLogEntry entry);
}

public record ConversationLogEntry(string ConversationId, string Sender, string Message, string DefinitionName, DateTime Timestamp);

/// <summary>
/// Asynchronous, concurrency-safe CSV conversation logger using a background queue.
/// </summary>
public class ConversationLogService : BackgroundService, IConversationLogService
{
    private readonly Channel<ConversationLogEntry> _channel;
    private readonly ILogger<ConversationLogService> _logger;
    private readonly string _csvPath;
    private readonly CsvConfiguration _csvConfig;

    public ConversationLogService(IConfiguration configuration, ILogger<ConversationLogService> logger)
    {
        _logger = logger;
        var folder = configuration.GetValue<string>("CsvOutputFolder") ?? Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(folder);
        _csvPath = Path.Combine(folder, "ConversationLogs.csv");
        _channel = Channel.CreateUnbounded<ConversationLogEntry>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Quote = '"',
            Escape = '"',
            Encoding = new UTF8Encoding(true),
            HasHeaderRecord = !File.Exists(_csvPath),
            ShouldQuote = _ => true
        };
    }

    public ValueTask EnqueueAsync(ConversationLogEntry entry)
    {
        if (!_channel.Writer.TryWrite(entry))
        {
            _logger.LogWarning("Failed to enqueue conversation log entry for {ConversationId}", entry.ConversationId);
        }
        return ValueTask.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ConversationLogService started writing to {Path}", _csvPath);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                while (await _channel.Reader.WaitToReadAsync(stoppingToken))
                {
                    var batch = new List<ConversationLogEntry>(256);
                    while (batch.Count < 256 && _channel.Reader.TryRead(out var entry))
                    {
                        batch.Add(entry);
                    }
                    if (batch.Count == 0) continue;
                    await WriteBatchAsync(batch, stoppingToken);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConversationLogService loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task WriteBatchAsync(IEnumerable<ConversationLogEntry> entries, CancellationToken ct)
    {
        bool fileExists = File.Exists(_csvPath);
        // Ensure header flag correct if file just created
        _csvConfig.HasHeaderRecord = !fileExists;
        using var stream = new StreamWriter(_csvPath, append: true, encoding: _csvConfig.Encoding);
        using var csv = new CsvWriter(stream, _csvConfig);
        if (!fileExists)
        {
            csv.WriteHeader<ConversationLogEntry>();
            await csv.NextRecordAsync();
        }
        foreach (var e in entries)
        {
            // Flatten record to match header order
            csv.WriteRecord(e);
            await csv.NextRecordAsync();
        }
    }
}
