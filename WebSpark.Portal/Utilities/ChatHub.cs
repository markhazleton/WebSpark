using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptSpark.Domain.Service;
using System.Collections.Concurrent;
using System.Globalization;

namespace WebSpark.Portal.Utilities;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IGPTDefinitionService _gptDefinitionService;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IConfiguration _configuration;
    private readonly IConversationLogService _conversationLogService;

    // Keeping initialization syntax as per instructions (using [])
    private static readonly Dictionary<string, string> GptPrompts = new();
    private static readonly object gptPromptsLock = new();

    public ChatHub(
        ILogger<ChatHub> logger,
        IGPTDefinitionService gptDefinitionService,
        IChatCompletionService chatCompletionService,
    IConfiguration configuration,
    IConversationLogService conversationLogService)
    {
        _logger = logger;
        _gptDefinitionService = gptDefinitionService;
        _chatCompletionService = chatCompletionService;
        _configuration = configuration;
        _conversationLogService = conversationLogService;
    }

    public class ChatEntry
    {
        public DateTime Timestamp { get; set; }
        public string User { get; set; } = string.Empty;
        public string UserMessage { get; set; } = string.Empty;
        public string BotResponse { get; set; } = string.Empty;
    }

    // Example LogEntry class; ensure this is defined in your project.
    public class LogEntry
    {
        public string ConversationId { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string DefinitionName { get; set; } = string.Empty;
    }

    private static readonly ConcurrentDictionary<string, List<ChatEntry>> ChatHistoryCache = new();

    public async Task SendMessage(string user, string message, string? conversationId = null, string variantName = "helpful")
    {
        // Validate parameters
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Invalid parameters: user or message is empty");
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

        // Generate message ID for client tracking
        string messageId = $"user-msg-{Guid.NewGuid()}";

        // Broadcast the user's message to the group
        await Clients.Group(conversationId).SendAsync("ReceiveMessage", user, message, conversationId, messageId, false);

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
        await _conversationLogService.EnqueueAsync(new ConversationLogEntry(conversationId, user, Sanitize(message), variantName, DateTime.UtcNow));
        await _conversationLogService.EnqueueAsync(new ConversationLogEntry(conversationId, variantName, Sanitize(botResponse), variantName, DateTime.UtcNow));
    }

    private async Task<string> GenerateStreamingBotResponse(ChatHistory chatHistory, string conversationId, string variantName)
    {
        var buffer = new StringBuilder();
        var message = new StringBuilder();
        bool isJsonMode = false;
        int jsonBraceCount = 0;
        string currentMessageId = $"msg-{Guid.NewGuid()}";

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
                            await Clients.Group(conversationId).SendAsync("ReceiveMessage", variantName, finalJson, conversationId, currentMessageId, false);

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
                        await Clients.Group(conversationId).SendAsync("ReceiveMessage", variantName, contentToSend, conversationId, currentMessageId, true);
                        message.Append(contentToSend);
                        buffer.Clear();
                    }
                }
            }

            // Send any non-JSON remaining buffer content
            if (!isJsonMode && buffer.Length > 0)
            {
                var remainingContent = buffer.ToString();
                await Clients.Group(conversationId).SendAsync("ReceiveMessage", variantName, remainingContent, conversationId, currentMessageId, true);
                message.Append(remainingContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in generating bot response");
            message.Append("An error occurred while processing your request.");
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "An error occurred while processing your request.", conversationId, $"error-{Guid.NewGuid()}", false);
        }

        return message.ToString();
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("New client connected: {ConnectionId}", connectionId);

        // Inform the client they're connected
        await Clients.Client(connectionId).SendAsync("ReceiveMessage",
            "System",
            "<p><i>Connected to chat server</i></p>",
            connectionId,
            $"conn-{Guid.NewGuid()}",
            false);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Client disconnected: {ConnectionId}, Reason: {Exception}",
            connectionId,
            exception?.Message ?? "No exception");

        await base.OnDisconnectedAsync(exception);
    }

    // CSV logging functionality
    private static string Sanitize(string value) => value.Replace('\r', ' ').Replace('\n', ' ').Trim();
}
