using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptSpark.Domain.Service;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace WebSpark.Portal.Utilities;

public class ChatHub(
    ILogger<ChatHub> logger,
    IGPTDefinitionService _gptDefinitionService,
    IChatCompletionService _chatCompletionService,
    IConfiguration _configuration) : Hub
{
    // Keeping initialization syntax as per instructions (using [])
    private static readonly Dictionary<string, string> GptPrompts = new();
    private static readonly object gptPromptsLock = new();

    public class ChatEntry
    {
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public string UserMessage { get; set; }
        public string BotResponse { get; set; }
    }

    // Example LogEntry class; ensure this is defined in your project.
    public class LogEntry
    {
        public string ConversationId { get; set; }
        public string Timestamp { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public string DefinitionName { get; set; }
    }

    private static readonly ConcurrentDictionary<string, List<ChatEntry>> ChatHistoryCache = new();

    public async Task SendMessage(string user, string message, string conversationId = null, string variantName = "helpful")
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
        {
            logger.LogWarning("Invalid parameters: user or message is empty");
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "Invalid message parameters.", conversationId);
            return;
        }

        // Use connection ID if conversationId not provided
        if (string.IsNullOrEmpty(conversationId))
        {
            conversationId = Context.ConnectionId;
        }

        // Add connection to the conversation group
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        // Ensure GPT definitions are loaded thread-safely
        if (GptPrompts.Count == 0)
        {
            var gptDefinitions = await _gptDefinitionService.GetDefinitionsAsync();
            lock (gptPromptsLock)
            {
                if (GptPrompts.Count == 0)
                {
                    foreach (var gptDefinition in gptDefinitions)
                    {
                        GptPrompts.Add(gptDefinition.Name, gptDefinition.Prompt);
                    }
                }
            }
        }

        // Retrieve or create chat history for this conversation
        if (!ChatHistoryCache.TryGetValue(conversationId, out List<ChatEntry>? chatHistoryList))
        {
            chatHistoryList = new List<ChatEntry>();
            ChatHistoryCache[conversationId] = chatHistoryList;
        }
        var timestamp = DateTime.Now;

        // Broadcast the user's message to the group
        await Clients.Group(conversationId).SendAsync("ReceiveMessage", user, message, conversationId);

        // Prepare the chat history for generating the bot response
        var systemPrompt = GptPrompts.ContainsKey(variantName)
            ? GptPrompts[variantName]
            : "You are a general GPT for conversation";
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddSystemMessage("You are a friendly and conversational assistant. Provide clear, engaging answers that invite further questions. Keep responses concise but leave room for curiosity, offering details that might naturally lead to follow-up questions. Use simple language, and if appropriate, suggest related topics to keep the conversation flowing.");
        foreach (var chatEntry in chatHistoryList)
        {
            chatHistory.AddUserMessage(chatEntry.UserMessage);
            chatHistory.AddSystemMessage(chatEntry.BotResponse);
        }
        chatHistory.AddUserMessage(message);

        // Generate the bot response with streaming
        var botResponse = await GenerateStreamingBotResponse(chatHistory, conversationId, variantName);

        // Create the chat entry and add it thread-safely
        var newChatEntry = new ChatEntry
        {
            Timestamp = timestamp,
            User = user,
            UserMessage = message,
            BotResponse = botResponse
        };
        lock (chatHistoryList)
        {
            chatHistoryList.Add(newChatEntry);
        }

        // Append both the user message and the bot response to the CSV log.
        // This will create two rows in the CSV file for the conversation.
        await AppendToCsvLog(conversationId, user, message, variantName);
        await AppendToCsvLog(conversationId, "System", botResponse, variantName);
    }

    private async Task<string> GenerateStreamingBotResponse(ChatHistory chatHistory, string conversationId, string variantName)
    {
        var buffer = new StringBuilder();
        var message = new StringBuilder();
        bool isJsonMode = false;
        int jsonBraceCount = 0;

        try
        {
            await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                if (response?.Content != null)
                {
                    var content = response.Content;

                    // Check if we're entering JSON mode
                    if (!isJsonMode && content.TrimStart().StartsWith("{"))
                    {
                        isJsonMode = true;
                        jsonBraceCount = 0;
                    }

                    if (isJsonMode)
                    {
                        // Track open and close braces to detect end of JSON
                        foreach (var ch in content)
                        {
                            if (ch == '{') jsonBraceCount++;
                            if (ch == '}') jsonBraceCount--;
                        }

                        message.Append(content);

                        // Check if JSON has ended
                        if (jsonBraceCount == 0)
                        {
                            // Send final accumulated JSON
                            var finalJson = message.ToString().Trim();
                            await Clients.Group(conversationId).SendAsync("ReceiveMessage", variantName, finalJson, conversationId);

                            buffer.Clear();
                            message.Clear();
                            isJsonMode = false;
                        }

                        continue; // Don't emit partial JSON chunks
                    }

                    // If not in JSON mode, stream response as normal
                    buffer.Append(content);

                    if (content.Contains('\n'))
                    {
                        var contentToSend = buffer.ToString();
                        await Clients.Group(conversationId).SendAsync("ReceiveMessage", variantName, contentToSend, conversationId);
                        message.Append(contentToSend);
                        buffer.Clear();
                    }
                }
            }

            // Send any non-JSON remaining buffer content
            if (!isJsonMode && buffer.Length > 0)
            {
                var remainingContent = buffer.ToString();
                await Clients.Group(conversationId).SendAsync("ReceiveMessage", variantName, remainingContent, conversationId);
                message.Append(remainingContent);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in generating bot response");
            message.Append("An error occurred while processing your request.");
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "An error occurred while processing your request.");
        }

        return message.ToString();
    }

    private async Task AppendToCsvLog(string conversationId, string sender, string message, string definitionName)
    {
        try
        {
            var csvOutputFolder = _configuration.GetValue<string>("CsvOutputFolder");
            if (string.IsNullOrEmpty(csvOutputFolder))
            {
                logger.LogError("CsvOutputFolder is not configured.");
                return;
            }

            Directory.CreateDirectory(csvOutputFolder);
            string csvFilePath = Path.Combine(csvOutputFolder, "ConversationLogs.csv");
            bool fileExists = System.IO.File.Exists(csvFilePath);

            // Clean message of line breaks
            var cleanedMessage = message.Replace("\r", " ").Replace("\n", " ").Trim();

            var logEntry = new LogEntry
            {
                ConversationId = conversationId,
                Timestamp = DateTime.UtcNow.ToString("O"),
                Sender = sender,
                Message = cleanedMessage,
                DefinitionName = definitionName
            };

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Quote = '"',
                Escape = '"',
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), // UTF-8 BOM for Excel
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
}
