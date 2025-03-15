using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptSpark.Domain.Service;
using System.Collections.Concurrent;
using System.Text;

namespace WebSpark.Portal.Utilities;

public class ChatHub(
    ILogger<ChatHub> logger,
    IGPTDefinitionService _gptDefinitionService,
    IChatCompletionService _chatCompletionService) : Hub
{
    // Keeping initialization syntax as per instructions (using [])
    private static readonly Dictionary<string, string> GptPrompts = [];
    private static readonly object gptPromptsLock = new object();

    public class ChatEntry
    {
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public string UserMessage { get; set; }
        public string BotResponse { get; set; }
    }

    private static readonly ConcurrentDictionary<string, List<ChatEntry>> ChatHistoryCache = new();

    public async Task SendMessage(string user, string message, string conversationId = null, string variantName = "helpful")
    {
        // Parameter validation
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(message))
        {
            logger.LogWarning("Invalid parameters: user or message is empty");
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "Invalid message parameters.", conversationId);
            return;
        }

        // Determine conversation ID, defaulting to the connection ID if not provided
        if (string.IsNullOrEmpty(conversationId))
        {
            conversationId = Context.ConnectionId;
        }

        // Add the connection to the conversation group so that messages are scoped correctly
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        // Ensure GPT definitions are loaded in a thread-safe manner
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
            chatHistoryList = [];
            ChatHistoryCache[conversationId] = chatHistoryList;
        }
        var timestamp = DateTime.Now;

        // Broadcast the user's message only to the clients in the conversation group
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

        // Generate bot response with streaming
        var botResponse = await GenerateStreamingBotResponse(chatHistory, conversationId, variantName);

        // Thread-safe addition of the new chat entry to the conversation history
        lock (chatHistoryList)
        {
            chatHistoryList.Add(new ChatEntry
            {
                Timestamp = timestamp,
                User = user,
                UserMessage = message,
                BotResponse = botResponse
            });
        }
    }

    private async Task<string> GenerateStreamingBotResponse(ChatHistory chatHistory, string conversationId, string variantName)
    {
        var buffer = new StringBuilder();
        var message = new StringBuilder();
        try
        {
            await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                if (response?.Content != null)
                {
                    buffer.Append(response.Content);

                    // Check for line breaks to send content in chunks
                    if (response.Content.Contains('\n'))
                    {
                        var contentToSend = buffer.ToString();
                        await Clients.Group(conversationId).SendAsync("ReceiveMessage", variantName, contentToSend, conversationId);
                        message.Append(contentToSend);
                        buffer.Clear();
                    }
                }
            }

            // Send any remaining content in the buffer
            if (buffer.Length > 0)
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
        LogConversation(conversationId, "System", message.ToString());
        return message.ToString();
    }

    private void LogConversation(string conversationId, string sender, string message)
    {
        logger.LogInformation("{Timestamp}, {ConversationId}, {Sender}: {Message}", DateTime.Now, conversationId, sender, message);
    }
}
