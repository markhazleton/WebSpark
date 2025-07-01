using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
namespace DataSpark.Web.Services;

public class UploadedCsvFile
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public long FileSizeBytes { get; set; }
}

public class OpenAIFileAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIFileAnalysisService> _logger;
    private const string BaseUrl = "https://api.openai.com/v1";
    private readonly List<UploadedCsvFile> _uploadedFiles = new();

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
    /// <summary>
    /// Analyze a CSV file. If keepFileUploaded is true, the file will be added to the uploaded files list.
    /// </summary>
    public async Task<string> AnalyzeCsvFileAsync(string filePath, string userPrompt, bool keepFileUploaded = false)
    {
        try
        {
            _logger.LogInformation("Starting CSV file analysis for file: {FilePath}", filePath);

            string fileId = await UploadFileAsync(filePath);
            _logger.LogInformation("File uploaded successfully with ID: {FileId}", fileId);

            // If keepFileUploaded is true, register the file
            if (keepFileUploaded)
            {
                var fileInfo = new FileInfo(filePath);
                var uploadedFile = new UploadedCsvFile
                {
                    FileId = fileId,
                    FileName = Path.GetFileName(filePath),
                    OriginalFilePath = filePath,
                    UploadedAt = DateTime.UtcNow,
                    FileSizeBytes = fileInfo.Length
                };
                _uploadedFiles.Add(uploadedFile);
                _logger.LogInformation("File registered for future use: {FileName}", uploadedFile.FileName);
            }

            string threadId = await CreateThreadAsync();
            _logger.LogInformation("Thread created with ID: {ThreadId}", threadId);

            await PostMessageWithFileAsync(threadId, userPrompt, fileId);
            _logger.LogInformation("Message posted to thread");

            string runId = await CreateRunAsync(threadId);
            _logger.LogInformation("Run created with ID: {RunId}", runId);

            await WaitForRunCompletionAsync(threadId, runId);
            _logger.LogInformation("Run completed successfully");

            string response = await GetLatestAssistantResponseAsync(threadId);

            // Only clean up the uploaded file if we're not keeping it
            if (!keepFileUploaded)
            {
                await DeleteFileAsync(fileId);
                _logger.LogInformation("Uploaded file cleaned up");
            }

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

            // Wait for file to be processed
            await WaitForFileProcessingAsync(fileId);

            // Verify file is accessible
            bool isAccessible = await VerifyFileUploadAsync(fileId);
            if (!isAccessible)
            {
                throw new InvalidOperationException($"File {fileId} was uploaded but is not accessible for analysis");
            }

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
            // Enhance the prompt to be more explicit about CSV analysis
            var enhancedPrompt = $@"I have uploaded a CSV file with ID: {fileId}. Please analyze this CSV file and {userPrompt}

Important instructions:
1. Load and examine the CSV file data structure
2. Display the first few rows to understand the data format
3. Provide summary statistics for numerical columns
4. Identify any data quality issues
5. Provide insights and analysis as requested

Please use the code interpreter tool to read and analyze the CSV file.";

            var payload = new
            {
                role = "user",
                content = new[]
                {
                    new { type = "text", text = enhancedPrompt }
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

    /// <summary>
    /// Upload a CSV file to OpenAI and add it to the list of available files
    /// </summary>
    public async Task<UploadedCsvFile> UploadAndRegisterCsvFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Uploading and registering CSV file: {FilePath}", filePath);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var fileInfo = new FileInfo(filePath);
            string fileId = await UploadFileAsync(filePath);

            var uploadedFile = new UploadedCsvFile
            {
                FileId = fileId,
                FileName = Path.GetFileName(filePath),
                OriginalFilePath = filePath,
                UploadedAt = DateTime.UtcNow,
                FileSizeBytes = fileInfo.Length
            };

            _uploadedFiles.Add(uploadedFile);
            _logger.LogInformation("File uploaded and registered successfully: {FileName} with ID: {FileId}", uploadedFile.FileName, fileId);

            return uploadedFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading and registering CSV file: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Get list of all uploaded CSV files
    /// </summary>
    public IReadOnlyList<UploadedCsvFile> GetUploadedFiles()
    {
        return _uploadedFiles.AsReadOnly();
    }

    /// <summary>
    /// Analyze multiple CSV files that have been previously uploaded
    /// </summary>
    public async Task<string> AnalyzeUploadedCsvFilesAsync(List<string> fileIds, string userPrompt)
    {
        try
        {
            if (fileIds == null || !fileIds.Any())
                throw new ArgumentException("At least one file ID must be provided");

            // Verify all file IDs exist in our uploaded files list
            var availableFileIds = _uploadedFiles.Select(f => f.FileId).ToHashSet();
            var missingFileIds = fileIds.Where(id => !availableFileIds.Contains(id)).ToList();

            if (missingFileIds.Any())
            {
                throw new ArgumentException($"The following file IDs are not in the uploaded files list: {string.Join(", ", missingFileIds)}");
            }

            _logger.LogInformation("Starting analysis of {Count} uploaded CSV files", fileIds.Count);

            string threadId = await CreateThreadAsync();
            _logger.LogInformation("Thread created with ID: {ThreadId}", threadId);

            await PostMessageWithMultipleFilesAsync(threadId, userPrompt, fileIds);
            _logger.LogInformation("Message posted to thread with {Count} files", fileIds.Count);

            string runId = await CreateRunAsync(threadId);
            _logger.LogInformation("Run created with ID: {RunId}", runId);

            await WaitForRunCompletionAsync(threadId, runId);
            _logger.LogInformation("Run completed successfully");

            string response = await GetLatestAssistantResponseAsync(threadId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing uploaded CSV files");
            throw;
        }
    }

    /// <summary>
    /// Analyze specific uploaded CSV files by their names
    /// </summary>
    public async Task<string> AnalyzeUploadedCsvFilesByNameAsync(List<string> fileNames, string userPrompt)
    {
        try
        {
            if (fileNames == null || !fileNames.Any())
                throw new ArgumentException("At least one file name must be provided");

            var fileIds = new List<string>();
            var missingFiles = new List<string>();

            foreach (var fileName in fileNames)
            {
                var uploadedFile = _uploadedFiles.FirstOrDefault(f =>
                    f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

                if (uploadedFile != null)
                {
                    fileIds.Add(uploadedFile.FileId);
                }
                else
                {
                    missingFiles.Add(fileName);
                }
            }

            if (missingFiles.Any())
            {
                throw new ArgumentException($"The following files are not in the uploaded files list: {string.Join(", ", missingFiles)}");
            }

            return await AnalyzeUploadedCsvFilesAsync(fileIds, userPrompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing uploaded CSV files by name");
            throw;
        }
    }

    /// <summary>
    /// Analyze all uploaded CSV files
    /// </summary>
    public async Task<string> AnalyzeAllUploadedCsvFilesAsync(string userPrompt)
    {
        try
        {
            if (!_uploadedFiles.Any())
                throw new InvalidOperationException("No CSV files have been uploaded yet");

            var fileIds = _uploadedFiles.Select(f => f.FileId).ToList();
            return await AnalyzeUploadedCsvFilesAsync(fileIds, userPrompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing all uploaded CSV files");
            throw;
        }
    }

    /// <summary>
    /// Remove a file from the uploaded files list and optionally delete it from OpenAI
    /// </summary>
    public async Task<bool> RemoveUploadedFileAsync(string fileId, bool deleteFromOpenAI = true)
    {
        try
        {
            var file = _uploadedFiles.FirstOrDefault(f => f.FileId == fileId);
            if (file == null)
            {
                _logger.LogWarning("File with ID {FileId} not found in uploaded files list", fileId);
                return false;
            }

            if (deleteFromOpenAI)
            {
                await DeleteFileAsync(fileId);
            }

            _uploadedFiles.Remove(file);
            _logger.LogInformation("File {FileName} (ID: {FileId}) removed from uploaded files list", file.FileName, fileId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing uploaded file: {FileId}", fileId);
            throw;
        }
    }

    /// <summary>
    /// Clear all uploaded files and optionally delete them from OpenAI
    /// </summary>
    public async Task ClearAllUploadedFilesAsync(bool deleteFromOpenAI = true)
    {
        try
        {
            _logger.LogInformation("Clearing {Count} uploaded files", _uploadedFiles.Count);

            if (deleteFromOpenAI)
            {
                var deleteTasks = _uploadedFiles.Select(f => DeleteFileAsync(f.FileId));
                await Task.WhenAll(deleteTasks);
            }

            _uploadedFiles.Clear();
            _logger.LogInformation("All uploaded files cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing uploaded files");
            throw;
        }
    }

    private async Task PostMessageWithMultipleFilesAsync(string threadId, string userPrompt, List<string> fileIds)
    {
        try
        {
            var attachments = fileIds.Select(fileId => new
            {
                file_id = fileId,
                tools = new[] { new { type = "code_interpreter" } }
            }).ToArray();

            // Enhance the prompt for multiple files
            var fileList = string.Join(", ", fileIds.Select((id, index) => $"File {index + 1}: {id}"));
            var enhancedPrompt = $@"I have uploaded {fileIds.Count} CSV files with the following IDs: {fileList}

Please analyze these CSV files and {userPrompt}

Important instructions:
1. Load and examine each CSV file data structure
2. Display the first few rows of each file to understand the data formats
3. Compare the data structures between files
4. Provide summary statistics for numerical columns in each file
5. Identify any data quality issues in each file
6. Provide comparative analysis and insights as requested
7. Highlight similarities and differences between the datasets

Please use the code interpreter tool to read and analyze all CSV files.";

            var payload = new
            {
                role = "user",
                content = new[]
                {
                    new { type = "text", text = enhancedPrompt }
                },
                attachments = attachments
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/threads/{threadId}/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI message posting failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API returned {response.StatusCode}: {errorContent}");
            }

            _logger.LogDebug("Message posted successfully to thread {ThreadId} with {Count} files", threadId, fileIds.Count);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to serialize message payload");
            throw new InvalidOperationException("Error serializing message payload", ex);
        }
        catch (Exception ex) when (!(ex is HttpRequestException))
        {
            _logger.LogError(ex, "Error posting message with multiple files");
            throw new InvalidOperationException($"Error posting message with multiple files: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Verify that a file was uploaded successfully and is accessible
    /// </summary>
    private async Task<bool> VerifyFileUploadAsync(string fileId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/files/{fileId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("File verification failed for {FileId}: {StatusCode}", fileId, response.StatusCode);
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var status = result.GetProperty("status").GetString();
            var purpose = result.GetProperty("purpose").GetString();

            _logger.LogInformation("File {FileId} verified - Status: {Status}, Purpose: {Purpose}", fileId, status, purpose);

            return status == "processed" && purpose == "assistants";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying file upload: {FileId}", fileId);
            return false;
        }
    }

    /// <summary>
    /// Wait for file to be processed by OpenAI
    /// </summary>
    private async Task WaitForFileProcessingAsync(string fileId, int maxWaitSeconds = 30)
    {
        var attempt = 0;
        var maxAttempts = maxWaitSeconds;

        while (attempt < maxAttempts)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/files/{fileId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var status = result.GetProperty("status").GetString();

                    _logger.LogDebug("File {FileId} status: {Status} (attempt {Attempt})", fileId, status, attempt + 1);

                    if (status == "processed")
                    {
                        _logger.LogInformation("File {FileId} processed successfully", fileId);
                        return;
                    }

                    if (status == "error")
                    {
                        var error = result.TryGetProperty("status_details", out var errorElement)
                            ? errorElement.GetString()
                            : "Unknown error";
                        throw new InvalidOperationException($"File processing failed: {error}");
                    }
                }

                await Task.Delay(1000);
                attempt++;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing file status response");
                throw;
            }
        }

        throw new TimeoutException($"File {fileId} was not processed within {maxWaitSeconds} seconds");
    }

    /// <summary>
    /// Verify that the assistant has the code_interpreter tool enabled
    /// </summary>
    private async Task<bool> VerifyAssistantConfigurationAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/assistants/{_options.AssistantId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve assistant configuration: {StatusCode}", response.StatusCode);
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (result.TryGetProperty("tools", out var toolsElement))
            {
                foreach (var tool in toolsElement.EnumerateArray())
                {
                    if (tool.TryGetProperty("type", out var typeElement) &&
                        typeElement.GetString() == "code_interpreter")
                    {
                        _logger.LogInformation("Assistant {AssistantId} has code_interpreter tool enabled", _options.AssistantId);
                        return true;
                    }
                }
            }

            _logger.LogWarning("Assistant {AssistantId} does not have code_interpreter tool enabled", _options.AssistantId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying assistant configuration");
            return false;
        }
    }

    /// <summary>
    /// Enhanced CSV file analysis with better error handling and verification
    /// </summary>
    public async Task<string> AnalyzeCsvFileWithVerificationAsync(string filePath, string userPrompt, bool keepFileUploaded = false)
    {
        try
        {
            _logger.LogInformation("Starting enhanced CSV file analysis for file: {FilePath}", filePath);

            // Verify assistant configuration
            bool assistantConfigured = await VerifyAssistantConfigurationAsync();
            if (!assistantConfigured)
            {
                throw new InvalidOperationException("Assistant is not properly configured with code_interpreter tool. Please check the assistant configuration in OpenAI.");
            }

            string fileId = await UploadFileAsync(filePath);
            _logger.LogInformation("File uploaded and verified successfully with ID: {FileId}", fileId);

            // If keepFileUploaded is true, register the file
            if (keepFileUploaded)
            {
                var fileInfo = new FileInfo(filePath);
                var uploadedFile = new UploadedCsvFile
                {
                    FileId = fileId,
                    FileName = Path.GetFileName(filePath),
                    OriginalFilePath = filePath,
                    UploadedAt = DateTime.UtcNow,
                    FileSizeBytes = fileInfo.Length
                };
                _uploadedFiles.Add(uploadedFile);
                _logger.LogInformation("File registered for future use: {FileName}", uploadedFile.FileName);
            }

            string threadId = await CreateThreadAsync();
            _logger.LogInformation("Thread created with ID: {ThreadId}", threadId);

            await PostMessageWithFileAsync(threadId, userPrompt, fileId);
            _logger.LogInformation("Message posted to thread");

            string runId = await CreateRunAsync(threadId);
            _logger.LogInformation("Run created with ID: {RunId}", runId);

            await WaitForRunCompletionAsync(threadId, runId);
            _logger.LogInformation("Run completed successfully");

            string response = await GetLatestAssistantResponseAsync(threadId);

            // Only clean up the uploaded file if we're not keeping it
            if (!keepFileUploaded)
            {
                await DeleteFileAsync(fileId);
                _logger.LogInformation("Uploaded file cleaned up");
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in enhanced CSV file analysis: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Diagnostic method to test the OpenAI configuration and file processing
    /// </summary>
    public async Task<string> DiagnoseConfigurationAsync()
    {
        var diagnostics = new StringBuilder();
        diagnostics.AppendLine("=== OpenAI Service Diagnostics ===");

        try
        {
            // Test 1: Check API key format
            diagnostics.AppendLine($"✓ API Key format: {(_options.ApiKey.StartsWith("sk-") ? "Valid" : "Invalid")}");

            // Test 2: Check Assistant ID format
            diagnostics.AppendLine($"✓ Assistant ID format: {(_options.AssistantId.StartsWith("asst_") ? "Valid" : "Invalid")}");

            // Test 3: Test API connectivity
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/models");
                diagnostics.AppendLine($"✓ API Connectivity: {(response.IsSuccessStatusCode ? "Connected" : $"Failed ({response.StatusCode})")}");
            }
            catch (Exception ex)
            {
                diagnostics.AppendLine($"✗ API Connectivity: Failed - {ex.Message}");
            }

            // Test 4: Check assistant configuration
            bool assistantValid = await VerifyAssistantConfigurationAsync();
            diagnostics.AppendLine($"✓ Assistant Configuration: {(assistantValid ? "Valid (has code_interpreter)" : "Invalid (missing code_interpreter tool)")}");

            // Test 5: List uploaded files
            diagnostics.AppendLine($"✓ Uploaded Files Count: {_uploadedFiles.Count}");
            if (_uploadedFiles.Any())
            {
                diagnostics.AppendLine("   Files:");
                foreach (var file in _uploadedFiles)
                {
                    diagnostics.AppendLine($"   - {file.FileName} (ID: {file.FileId}, Size: {file.FileSizeBytes} bytes)");
                }
            }

            diagnostics.AppendLine("=== End Diagnostics ===");
        }
        catch (Exception ex)
        {
            diagnostics.AppendLine($"✗ Diagnostic failed: {ex.Message}");
        }

        return diagnostics.ToString();
    }
}
