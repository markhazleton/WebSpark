using CsvHelper;
using CsvHelper.Configuration;
using Markdig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using PromptSpark.Domain.Models;
using PromptSpark.Domain.Service;
using System.Globalization;

namespace WebSpark.Portal.Areas.PromptSpark.Controllers;

public class ChatCompletionController(
    IHttpContextAccessor _httpContextAccessor,
    IHubContext<ChatHub> hubContext,
    IChatCompletionService _chatCompletionService,
    IGPTDefinitionService definitionService,
    ILogger<ChatCompletionController> logger,
    IConfiguration configuration) : PromptSparkBaseController
{

    private async Task AppendToCsvLog(string conversationId, string sender, string message, string definitionName)
    {
        try
        {
            // Index the CSV output folder from configuration
            var csvOutputFolder = configuration.GetValue<string>("CsvOutputFolder");
            if (string.IsNullOrEmpty(csvOutputFolder))
            {
                logger.LogError("CsvOutputFolder is not configured.");
                return;
            }

            // Ensure the directory exists
            Directory.CreateDirectory(csvOutputFolder);
            string csvFilePath = Path.Combine(csvOutputFolder, "ConversationLogs.csv");

            // Check if the file exists to determine if the header should be written
            bool fileExists = System.IO.File.Exists(csvFilePath);

            // Prepare the log entry as an object
            var logEntry = new LogEntry
            {
                ConversationId = conversationId,
                Timestamp = DateTime.UtcNow.ToString("O"),
                Sender = sender,
                Message = message,
                DefinitionName = definitionName
            };

            // Configure CsvHelper with UTF-8 encoding and to handle quoting on all fields
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Quote = '"',
                Escape = '"',
                Encoding = new UTF8Encoding(true), // UTF-8 with BOM
                HasHeaderRecord = !fileExists, // Write header only if the file is new
                ShouldQuote = args => true // Force quotes on all fields
            };

            // Append the log entry to the CSV file
            using var stream = new StreamWriter(csvFilePath, append: true, encoding: csvConfig.Encoding);
            using var csvWriter = new CsvWriter(stream, csvConfig);
            if (!fileExists) // Write the header only if the file did not exist
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

    public async Task<IActionResult> Index(int id = 0)
    {
        logger.LogInformation("Entering Index method with id: {Id}", id);
        try
        {
            if (id == 0)
            {
                // Index list of definitions
                var definitions = await definitionService.GetDefinitionsAsync();
                logger.LogInformation("Retrieved {Count} definitions.", definitions.Count);

                // Return to pick definition view
                return View("PickDefinition", definitions);
            }

            var definitionDto = await definitionService.GetDefinitionDtoAsync(id);
            if (definitionDto == null)
            {
                logger.LogWarning("No definition found for id: {Id}", id);
                return NotFound("Definition not found");
            }

            var session = _httpContextAccessor.HttpContext.Session;
            session.SetString("DefinitionDto", JsonConvert.SerializeObject(definitionDto));
            logger.LogInformation("Definition stored in session for id: {Id}", id);

            return View(definitionDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing Index with id: {Id}", id);
            return View("Error"); // Return a user-friendly error view
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromForm] string message, [FromForm] string conversationHistory)
    {
        logger.LogInformation("Entering SendMessage method with message: {Message} and conversation history length: {Length}", message, conversationHistory?.Length);

        if (string.IsNullOrEmpty(message))
        {
            logger.LogWarning("Received empty message.");
            return BadRequest("Message cannot be empty");
        }

        var session = _httpContextAccessor.HttpContext.Session;
        var definitionDtoJson = session.GetString("DefinitionDto");

        if (string.IsNullOrEmpty(definitionDtoJson))
        {
            logger.LogWarning("DefinitionDto not found in session.");
            return BadRequest("Session expired or invalid");
        }

        var definitionDto = JsonConvert.DeserializeObject<DefinitionDto>(definitionDtoJson);
        if (definitionDto == null)
        {
            logger.LogError("Failed to deserialize DefinitionDto from session.");
            return BadRequest("Invalid session data");
        }

        logger.LogInformation("Processing message for definition: {Name}", definitionDto.Name);

        // Generate or retrieve a unique conversation identifier
        string conversationId = session.GetString("ConversationId");
        if (string.IsNullOrEmpty(conversationId) || conversationHistory.Length == 1)
        {
            conversationId = Guid.NewGuid().ToString(); // Generate a new unique identifier for new conversations
            session.SetString("ConversationId", conversationId);
            logger.LogInformation("Generated new conversation ID: {ConversationId}", conversationId);
        }

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(definitionDto.Prompt);
        chatHistory.AddSystemMessage("You are in a conversation, keep your answers brief, always ask follow-up questions.");

        // Parse the conversationHistory JSON string into a list of messages
        List<string> messages;
        try
        {
            messages = JsonConvert.DeserializeObject<List<string>>(conversationHistory);
            logger.LogInformation("Parsed conversation history with {Count} messages.", messages?.Count ?? 0);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse conversation history JSON.");
            return BadRequest("Invalid conversation history format");
        }

        // Populate ChatHistory with messages from conversationHistory
        for (int i = 0; i < messages.Count; i++)
        {
            if (i % 2 == 0)
            {
                chatHistory.AddUserMessage(messages[i]); // Even index - User
                await AppendToCsvLog(conversationId, "User", messages[i], definitionDto.Name); // Log user message
            }
            else
            {
                chatHistory.AddSystemMessage(messages[i]); // Odd index - System
                await AppendToCsvLog(conversationId, "System", messages[i], definitionDto.Name); // Log system message
            }
        }

        try
        {
            var buffer = new StringBuilder();

            await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                if (response?.Content != null)
                {
                    buffer.Append(response.Content);
                    if (response.Content.Contains('\n'))
                    {
                        var contentToSend = buffer.ToString();
                        var htmlContent = Markdown.ToHtml(contentToSend);
                        await hubContext.Clients.All.SendAsync("ReceiveMessage", "System", htmlContent);
                        logger.LogInformation("Sent message to client: {Message}", contentToSend);

                        // Append system message to CSV
                        await AppendToCsvLog(conversationId, "System", contentToSend, definitionDto.Name);

                        buffer.Clear();
                    }
                }
            }

            // If there's any remaining content in the buffer, send it
            if (buffer.Length > 0)
            {
                var remainingContent = buffer.ToString();
                var htmlContent = Markdown.ToHtml(remainingContent);
                await hubContext.Clients.All.SendAsync("ReceiveMessage", "System", htmlContent);
                logger.LogInformation("Sent remaining content to client: {RemainingContent}", remainingContent);

                // Append remaining content to CSV
                await AppendToCsvLog(conversationId, "System", remainingContent, definitionDto.Name);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing the chat completion request.");
            return StatusCode(500, "An error occurred while processing your request.");
        }

        logger.LogInformation("SendMessage completed successfully.");
        return Ok();
    }

    // Define the class to represent a log entry
    public class LogEntry
    {
        public string ConversationId { get; set; }
        public string DefinitionName { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Timestamp { get; set; }
    }
}
