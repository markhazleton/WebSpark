using CsvHelper;
using CsvHelper.Configuration;
using Markdig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using PromptSpark.Domain.Models;
using PromptSpark.Domain.Service;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace WebSpark.Portal.Areas.PromptSpark.Controllers;

[Area("PromptSpark")]
[Route("PromptSpark/EnhancedChat")]
[EnableRateLimiting("ChatCompletionPolicy")]
public class EnhancedChatCompletionController(
    IHttpContextAccessor _httpContextAccessor,
    IHubContext<ChatHub> hubContext,
    IChatCompletionService _chatCompletionService,
    IGPTDefinitionService definitionService,
    ILogger<EnhancedChatCompletionController> logger,
    IConfiguration configuration,
    IMemoryCache memoryCache) : PromptSparkBaseController
{
    private const int MaxConversationLength = 50; // Limit conversation history
    private const int MaxMessageLength = 2000;
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

    // Enhanced message validation
    private static bool IsValidMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return false;
        if (message.Length > MaxMessageLength) return false;

        // Check for potentially harmful content patterns
        var suspiciousPatterns = new[] { "<script", "javascript:", "data:", "vbscript:" };
        return !suspiciousPatterns.Any(pattern =>
            message.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    // Enhanced conversation history management
    private static List<string> TrimConversationHistory(List<string> messages)
    {
        if (messages.Count <= MaxConversationLength) return messages;

        // Keep the system message and recent conversation
        var trimmed = new List<string>();

        // Always keep the first few messages (usually contain system context)
        var keepInitial = Math.Min(4, messages.Count);
        trimmed.AddRange(messages.Take(keepInitial));

        // Keep the most recent messages
        var recentCount = MaxConversationLength - keepInitial;
        if (recentCount > 0 && messages.Count > keepInitial)
        {
            trimmed.AddRange(messages.TakeLast(recentCount));
        }

        return trimmed;
    }

    [HttpGet("{id}/{slug}")]
    public async Task<IActionResult> Variant(int id = 0, string? slug = null)
    {
        logger.LogInformation("Accessing chat variant with id: {Id}, slug: {Slug}", id, slug);

        if (id == 0)
        {
            var definitions = await GetCachedDefinitionsAsync();
            logger.LogInformation("Retrieved {Count} definitions from cache/database.", definitions.Count);
            return View("PickDefinition", definitions);
        }

        var definitionDto = await GetCachedDefinitionAsync(id);
        if (definitionDto == null)
        {
            logger.LogWarning("Definition not found for id: {Id}", id);
            return RedirectToAction("Index");
        }

        if (!string.Equals(definitionDto.Slug, slug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToActionPermanent(nameof(Variant), new { id, slug = definitionDto.Slug });
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        session?.SetString("DefinitionDto", System.Text.Json.JsonSerializer.Serialize(definitionDto));

        logger.LogInformation("Definition {Name} loaded for chat variant", definitionDto.Name);
        return View(definitionDto);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("Accessing chat completion index");

        try
        {
            var definitions = await GetCachedDefinitionsAsync();
            logger.LogInformation("Retrieved {Count} definitions for selection.", definitions.Count);
            return View("PickDefinition", definitions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving definitions for index");
            return View("Error");
        }
    }

    [HttpPost]
    [Route("SendMessage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage([FromForm] SendMessageRequest request)
    {
        using var activity = logger.BeginScope("SendMessage for conversation {ConversationId}", request.ConversationId);

        logger.LogInformation("Processing message with length: {Length}", request.Message?.Length ?? 0);

        // Enhanced validation
        var validationResult = ValidateRequest(request);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Message validation failed: {Error}", validationResult.Error);
            return BadRequest(new { error = validationResult.Error, code = "VALIDATION_FAILED" });
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        var definitionDtoJson = session?.GetString("DefinitionDto");

        if (string.IsNullOrEmpty(definitionDtoJson))
        {
            logger.LogWarning("Session expired or invalid");
            return BadRequest(new { error = "Session expired. Please refresh the page.", code = "SESSION_EXPIRED" });
        }

        DefinitionDto? definitionDto;
        try
        {
            definitionDto = System.Text.Json.JsonSerializer.Deserialize<DefinitionDto>(definitionDtoJson);
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize DefinitionDto from session");
            return BadRequest(new { error = "Invalid session data", code = "SESSION_INVALID" });
        }

        if (definitionDto == null)
        {
            return BadRequest(new { error = "Invalid session data", code = "SESSION_INVALID" });
        }

        // Generate or retrieve conversation ID
        string conversationId = GetOrCreateConversationId(session, request.ConversationHistory?.Count ?? 0);

        var chatHistory = BuildChatHistory(definitionDto, request.ConversationHistory ?? new List<string>());

        try
        {
            await ProcessStreamingResponse(chatHistory, conversationId, definitionDto, request.Message ?? string.Empty);
            return Ok(new { success = true, conversationId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing chat completion request for conversation {ConversationId}", conversationId);

            // Send error message to client
            await hubContext.Clients.All.SendAsync("ReceiveMessage",
                "System",
                "I apologize, but I'm experiencing technical difficulties. Please try again in a moment.",
                conversationId,
                $"error-{Guid.NewGuid()}",
                false);

            return StatusCode(500, new
            {
                error = "An error occurred while processing your request. Please try again.",
                code = "PROCESSING_ERROR"
            });
        }
    }

    [HttpPost]
    [Route("ResetConversation")]
    public IActionResult ResetConversation()
    {
        logger.LogInformation("Resetting conversation");

        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session != null)
            {
                httpContext.Session.Remove("ConversationId");
                return Ok(new { success = true, message = "Conversation reset successfully" });
            }

            logger.LogWarning("HttpContext or Session is null when resetting conversation");
            return BadRequest(new { error = "Session not available", code = "SESSION_UNAVAILABLE" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting conversation");
            return StatusCode(500, new { error = "Failed to reset conversation", code = "RESET_ERROR" });
        }
    }

    [HttpGet]
    [Route("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "2.0"
        });
    }

    // Private helper methods
    private async Task<List<DefinitionDto>> GetCachedDefinitionsAsync()
    {
        const string cacheKey = "chat_definitions";

        if (memoryCache.TryGetValue(cacheKey, out List<DefinitionDto>? cachedDefinitions) && cachedDefinitions != null)
        {
            return cachedDefinitions;
        }

        var definitions = await definitionService.GetDefinitionsAsync();
        memoryCache.Set(cacheKey, definitions, CacheExpiry);

        return definitions;
    }

    private async Task<DefinitionDto?> GetCachedDefinitionAsync(int id)
    {
        string cacheKey = $"chat_definition_{id}";

        if (memoryCache.TryGetValue(cacheKey, out DefinitionDto? cachedDefinition) && cachedDefinition != null)
        {
            return cachedDefinition;
        }

        var definition = await definitionService.GetDefinitionDtoAsync(id);
        if (definition != null)
        {
            memoryCache.Set(cacheKey, definition, CacheExpiry);
        }

        return definition;
    }

    private static ValidationResult ValidateRequest(SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return ValidationResult.Invalid("Message cannot be empty");
        }

        if (!IsValidMessage(request.Message))
        {
            return ValidationResult.Invalid("Invalid message content");
        }

        if (request.ConversationHistory != null && request.ConversationHistory.Count > MaxConversationLength * 2)
        {
            return ValidationResult.Invalid("Conversation history too long");
        }

        return ValidationResult.Valid;
    }

    private string GetOrCreateConversationId(ISession? session, int historyLength)
    {
        string? conversationId = session?.GetString("ConversationId");

        if (string.IsNullOrEmpty(conversationId) || historyLength <= 1)
        {
            conversationId = Guid.NewGuid().ToString();
            session?.SetString("ConversationId", conversationId);
            logger.LogInformation("Generated new conversation ID: {ConversationId}", conversationId);
        }

        return conversationId;
    }

    private ChatHistory BuildChatHistory(DefinitionDto definitionDto, List<string> conversationHistory)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(definitionDto.Prompt);
        chatHistory.AddSystemMessage("You are in a conversation, keep your answers brief, always ask follow-up questions.");

        // Trim conversation history if too long
        var trimmedHistory = TrimConversationHistory(conversationHistory);

        for (int i = 0; i < trimmedHistory.Count; i++)
        {
            if (i % 2 == 0)
            {
                chatHistory.AddUserMessage(trimmedHistory[i]);
            }
            else
            {
                chatHistory.AddSystemMessage(trimmedHistory[i]);
            }
        }

        return chatHistory;
    }
    private async Task ProcessStreamingResponse(ChatHistory chatHistory, string conversationId, DefinitionDto definitionDto, string userMessage)
    {
        var buffer = new StringBuilder();
        var rawMarkdownBuffer = new StringBuilder();
        var messageId = Guid.NewGuid().ToString();
        var isFirstChunk = true;
        var fullHtmlContent = new StringBuilder();

        // Add timeout for streaming
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory, cancellationToken: cancellationTokenSource.Token))
        {
            if (response?.Content != null)
            {
                buffer.Append(response.Content);
                rawMarkdownBuffer.Append(response.Content);

                // Send chunks more intelligently
                if (ShouldSendChunk(buffer.ToString(), isFirstChunk))
                {
                    var contentToSend = buffer.ToString();
                    fullHtmlContent.Append(contentToSend);

                    var htmlContent = Markdown.ToHtml(fullHtmlContent.ToString());

                    await hubContext.Clients.All.SendAsync("ReceiveMessage",
                        "System",
                        htmlContent,
                        conversationId,
                        messageId,
                        !isFirstChunk,
                        cancellationToken: cancellationTokenSource.Token);

                    if (isFirstChunk)
                    {
                        await AppendToCsvLog(conversationId, "System", "Streaming response started...", definitionDto.Name);
                        isFirstChunk = false;
                    }

                    buffer.Clear();
                }
            }
        }

        // Send any remaining content
        if (buffer.Length > 0)
        {
            var remainingContent = buffer.ToString();
            fullHtmlContent.Append(remainingContent);
            var htmlContent = Markdown.ToHtml(fullHtmlContent.ToString());

            await hubContext.Clients.All.SendAsync("ReceiveMessage",
                "System",
                htmlContent,
                conversationId,
                messageId,
                !isFirstChunk);
        }

        // Log complete response
        var fullResponse = rawMarkdownBuffer.ToString();
        if (!string.IsNullOrEmpty(fullResponse))
        {
            await AppendToCsvLog(conversationId, "System", fullResponse, definitionDto.Name);
        }

        // Log user message
        await AppendToCsvLog(conversationId, "User", userMessage, definitionDto.Name);
    }

    private static bool ShouldSendChunk(string content, bool isFirstChunk)
    {
        // Send first chunk quickly for responsiveness
        if (isFirstChunk && content.Length > 10) return true;

        // Send on natural breaks
        if (content.Contains('\n') || content.Contains('.') && content.Length > 50) return true;

        // Send if buffer gets large
        if (content.Length > 100) return true;

        return false;
    }

    private async Task AppendToCsvLog(string conversationId, string sender, string message, string definitionName)
    {
        try
        {
            var csvOutputFolder = configuration.GetValue<string>("CsvOutputFolder");
            if (string.IsNullOrEmpty(csvOutputFolder))
            {
                logger.LogError("CsvOutputFolder is not configured.");
                return;
            }

            Directory.CreateDirectory(csvOutputFolder);
            string csvFilePath = Path.Combine(csvOutputFolder, "ConversationLogs.csv");
            bool fileExists = System.IO.File.Exists(csvFilePath);

            var logEntry = new LogEntry
            {
                ConversationId = conversationId,
                Timestamp = DateTime.UtcNow.ToString("O"),
                Sender = sender,
                Message = message.Replace("\r", " ").Replace("\n", " ").Trim(),
                DefinitionName = definitionName
            };

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Quote = '"',
                Escape = '"',
                Encoding = new UTF8Encoding(true),
                HasHeaderRecord = !fileExists,
                ShouldQuote = args => true
            };

            using var stream = new StreamWriter(csvFilePath, append: true, encoding: csvConfig.Encoding);
            using var csvWriter = new CsvWriter(stream, csvConfig);

            if (!fileExists)
            {
                csvWriter.WriteHeader<LogEntry>();
                await csvWriter.NextRecordAsync();
            }

            csvWriter.WriteRecord(logEntry);
            await csvWriter.NextRecordAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while writing to CSV log.");
        }
    }

    // Supporting classes
    public class SendMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<string>? ConversationHistory { get; set; }
        public string? ConversationId { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string? Error { get; private set; }

        private ValidationResult(bool isValid, string? error = null)
        {
            IsValid = isValid;
            Error = error;
        }

        public static ValidationResult Valid => new(true);
        public static ValidationResult Invalid(string error) => new(false, error);
    }

    public class LogEntry
    {
        public required string ConversationId { get; set; }
        public required string DefinitionName { get; set; }
        public required string Message { get; set; }
        public required string Sender { get; set; }
        public required string Timestamp { get; set; }
    }
}
