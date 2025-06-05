using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
namespace DataSpark.Web.Services;


public class OpenAIFileAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIFileAnalysisService> _logger;
    private const string BaseUrl = "https://api.openai.com/v1";

    public class OpenAIOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string AssistantId { get; set; } = string.Empty;
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromMinutes(5);
    }
    public OpenAIFileAnalysisService(HttpClient httpClient, IOptions<OpenAIOptions> options, ILogger<OpenAIFileAnalysisService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Configure HTTP client timeout
        _httpClient.Timeout = _options.HttpTimeout;

        // Validate configuration
        if (string.IsNullOrEmpty(_options.ApiKey))
            throw new InvalidOperationException("OpenAI ApiKey is not configured.");

        if (string.IsNullOrEmpty(_options.AssistantId))
            throw new InvalidOperationException("OpenAI AssistantId is not configured.");

        if (!_options.ApiKey.StartsWith("sk-"))
            throw new InvalidOperationException("OpenAI ApiKey must start with 'sk-'. Please check your configuration.");

        if (!_options.AssistantId.StartsWith("asst_"))
            throw new InvalidOperationException("OpenAI AssistantId must start with 'asst_'. Please check your configuration.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
    }
    public async Task<string> AnalyzeCsvFileAsync(string filePath, string userPrompt)
    {
        try
        {
            _logger.LogInformation("Starting CSV file analysis for file: {FilePath}", filePath);

            string fileId = await UploadFileAsync(filePath);
            _logger.LogInformation("File uploaded successfully with ID: {FileId}", fileId);

            string threadId = await CreateThreadAsync();
            _logger.LogInformation("Thread created with ID: {ThreadId}", threadId);

            await PostMessageWithFileAsync(threadId, userPrompt, fileId);
            _logger.LogInformation("Message posted to thread");

            string runId = await CreateRunAsync(threadId);
            _logger.LogInformation("Run created with ID: {RunId}", runId);

            await WaitForRunCompletionAsync(threadId, runId);
            _logger.LogInformation("Run completed successfully");

            string response = await GetLatestAssistantResponseAsync(threadId);

            // Clean up the uploaded file
            await DeleteFileAsync(fileId);
            _logger.LogInformation("Uploaded file cleaned up");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing CSV file: {FilePath}", filePath);
            throw;
        }
    }
    private async Task<string> UploadFileAsync(string filePath)
    {
        try
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            // If the file has a .tmp extension, try to get the original name from a temp file pattern or fallback to .csv
            if (Path.GetExtension(fileName).Equals(".tmp", StringComparison.OrdinalIgnoreCase))
            {
                // Try to find a .csv in the original name (if your temp files are like original.csv.tmp)
                var originalName = fileName;
                if (originalName.EndsWith(".csv.tmp", StringComparison.OrdinalIgnoreCase))
                    fileName = originalName.Substring(0, originalName.Length - 4); // remove .tmp
                else
                    fileName = Path.ChangeExtension(fileName, ".csv"); // fallback to .csv
            }

            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

            var form = new MultipartFormDataContent
            {
                { fileContent, "file", fileName },
                { new StringContent("assistants"), "purpose" }
            };

            var response = await _httpClient.PostAsync($"{BaseUrl}/files", form);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI file upload failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var fileId = result.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(fileId))
            {
                throw new InvalidOperationException("File ID not returned from OpenAI API");
            }

            _logger.LogDebug("File uploaded successfully with ID: {FileId}", fileId);
            return fileId;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI API response for file upload");
            throw new InvalidOperationException("Invalid response format from OpenAI API", ex);
        }
        catch (Exception ex) when (!(ex is HttpRequestException))
        {
            _logger.LogError(ex, "Error uploading file to OpenAI");
            throw new InvalidOperationException($"Error uploading file to OpenAI: {ex.Message}", ex);
        }
    }
    private async Task<string> CreateThreadAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/threads", new StringContent("{}", Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI thread creation failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var threadId = result.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(threadId))
            {
                throw new InvalidOperationException("Thread ID not returned from OpenAI API");
            }

            _logger.LogDebug("Thread created successfully with ID: {ThreadId}", threadId);
            return threadId;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI API response for thread creation");
            throw new InvalidOperationException("Invalid response format from OpenAI API", ex);
        }
        catch (Exception ex) when (!(ex is HttpRequestException))
        {
            _logger.LogError(ex, "Error creating thread");
            throw new InvalidOperationException($"Error creating thread: {ex.Message}", ex);
        }
    }
    private async Task PostMessageWithFileAsync(string threadId, string userPrompt, string fileId)
    {
        try
        {
            var payload = new
            {
                role = "user",
                content = new[]
                {
                    new { type = "text", text = userPrompt }
                },
                attachments = new[]
                {
                    new
                    {
                        file_id = fileId,
                        tools = new[] { new { type = "code_interpreter" } }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/threads/{threadId}/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI message posting failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
            }

            _logger.LogDebug("Message posted successfully to thread {ThreadId}", threadId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to serialize message payload");
            throw new InvalidOperationException("Error serializing message payload", ex);
        }
        catch (Exception ex) when (!(ex is HttpRequestException))
        {
            _logger.LogError(ex, "Error posting message");
            throw new InvalidOperationException($"Error posting message: {ex.Message}", ex);
        }
    }
    private async Task<string> CreateRunAsync(string threadId)
    {
        try
        {
            var payload = new { assistant_id = _options.AssistantId };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/threads/{threadId}/runs", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI run creation failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var runId = result.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(runId))
            {
                throw new InvalidOperationException("Run ID not returned from OpenAI API");
            }

            _logger.LogDebug("Run created successfully with ID: {RunId}", runId);
            return runId;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI API response for run creation");
            throw new InvalidOperationException("Invalid response format from OpenAI API", ex);
        }
    }
    private async Task WaitForRunCompletionAsync(string threadId, string runId)
    {
        var maxAttempts = 120; // 2 minutes maximum wait time with 1-second intervals
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            await Task.Delay(1000);
            attempt++;

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/threads/{threadId}/runs/{runId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI run status check failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var status = result.GetProperty("status").GetString();

                _logger.LogDebug("Run {RunId} status: {Status} (attempt {Attempt})", runId, status, attempt);

                switch (status)
                {
                    case "completed":
                        _logger.LogInformation("Run {RunId} completed successfully", runId);
                        return;

                    case "failed":
                        var error = result.TryGetProperty("last_error", out var errorElement)
                            ? errorElement.GetProperty("message").GetString()
                            : "Unknown error";
                        _logger.LogError("Run {RunId} failed: {Error}", runId, error);
                        throw new InvalidOperationException($"OpenAI run failed: {error}");

                    case "cancelled":
                        _logger.LogWarning("Run {RunId} was cancelled", runId);
                        throw new InvalidOperationException("OpenAI run was cancelled");

                    case "expired":
                        _logger.LogWarning("Run {RunId} expired", runId);
                        throw new InvalidOperationException("OpenAI run expired");

                    case "requires_action":
                        _logger.LogWarning("Run {RunId} requires action - this is not supported in this implementation", runId);
                        throw new InvalidOperationException("OpenAI run requires action which is not supported");

                    case "queued":
                    case "in_progress":
                    case "cancelling":
                        // Continue waiting
                        break;

                    default:
                        _logger.LogWarning("Run {RunId} has unknown status: {Status}", runId, status);
                        break;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI API response for run status");
                throw new InvalidOperationException("Invalid response format from OpenAI API", ex);
            }
        }

        _logger.LogError("Run {RunId} did not complete within the maximum wait time", runId);
        throw new TimeoutException($"OpenAI run did not complete within {maxAttempts} seconds");
    }
    private async Task<string> GetLatestAssistantResponseAsync(string threadId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/threads/{threadId}/messages");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI messages retrieval failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var assistantMessages = new List<(DateTimeOffset createdAt, string text)>();

            foreach (var message in result.GetProperty("data").EnumerateArray())
            {
                var role = message.GetProperty("role").GetString();
                if (role == "assistant")
                {
                    var createdAt = message.TryGetProperty("created_at", out var createdAtElement) && createdAtElement.ValueKind == JsonValueKind.Number
                        ? DateTimeOffset.FromUnixTimeSeconds(createdAtElement.GetInt64())
                        : DateTimeOffset.MinValue;
                    var contentArray = message.GetProperty("content").EnumerateArray();
                    foreach (var contentItem in contentArray)
                    {
                        if (contentItem.TryGetProperty("text", out var textElement))
                        {
                            var responseText = textElement.GetProperty("value").GetString();
                            if (!string.IsNullOrEmpty(responseText))
                                assistantMessages.Add((createdAt, responseText));
                        }
                    }
                }
            }

            if (assistantMessages.Count == 0)
            {
                _logger.LogWarning("No assistant response found in thread {ThreadId}", threadId);
                return "No assistant response found.";
            }

            // Sort by creation time and concatenate all responses
            var fullResponse = string.Join("\n\n", assistantMessages.OrderBy(m => m.createdAt).Select(m => m.text));
            _logger.LogDebug("Retrieved {Count} assistant responses from thread {ThreadId}", assistantMessages.Count, threadId);
            return fullResponse;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI API response for messages");
            throw new InvalidOperationException("Invalid response format from OpenAI API", ex);
        }
    }

    private async Task DeleteFileAsync(string fileId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/files/{fileId}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("File {FileId} deleted successfully", fileId);
            }
            else
            {
                // Log but don't throw - file cleanup is not critical
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to delete file {FileId}: {StatusCode} {ErrorContent}", fileId, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw - file cleanup is not critical
            _logger.LogWarning(ex, "Error attempting to delete file {FileId}", fileId);
        }
    }
}
