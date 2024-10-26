using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;
using System.Text;

namespace PromptSpark.Chat.Hubs;

public class ChatHub(IChatCompletionService _chatCompletionService) : Hub
{
    public class ChatEntry
    {
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public string UserMessage { get; set; }
        public string BotResponse { get; set; }
    }

    private static readonly ConcurrentDictionary<string, List<ChatEntry>> ChatHistoryCache = new();
   

    public async Task SendMessage(string user, string message, string conversationId)
    {
        if (!ChatHistoryCache.ContainsKey(conversationId))
        {
            ChatHistoryCache[conversationId] = new List<ChatEntry>();
        }
        var timestamp = DateTime.Now;

        // Broadcast the user's message to all clients with conversation ID
        await Clients.All.SendAsync("ReceiveMessage", user, message, conversationId);

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are in a conversation, keep your answers brief, always ask follow-up questions, ask if ready for full answer.");

        foreach (var chatEntry in ChatHistoryCache[conversationId])
        {
            chatHistory.AddUserMessage(chatEntry.UserMessage);
            chatHistory.AddSystemMessage(chatEntry.BotResponse);
        }
        chatHistory.AddUserMessage(message);

        // Generate bot response with streaming
        var botResponse = await GenerateStreamingBotResponse(chatHistory, conversationId);

        // Add the message to the in-memory cache
        ChatHistoryCache[conversationId].Add(new ChatEntry
        {
            Timestamp = timestamp,
            User = user,
            UserMessage = message,
            BotResponse = botResponse
        });
    }

    private async Task<string> GenerateStreamingBotResponse(ChatHistory chatHistory, string conversationId)
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
                        await Clients.All.SendAsync("ReceiveMessage", "ChatBot", contentToSend, conversationId);
                        await AppendToCsvLog(conversationId, "System", contentToSend); // Log system message
                        message.Append(contentToSend);
                        buffer.Clear();
                    }
                }
            }

            // Send any remaining content in the buffer
            if (buffer.Length > 0)
            {
                var remainingContent = buffer.ToString();
                await Clients.All.SendAsync("ReceiveMessage", "ChatBot", remainingContent, conversationId);
                message.Append(remainingContent);
                await AppendToCsvLog(conversationId, "System", remainingContent); // Log remaining content
            }
        }
        catch (Exception ex)
        {
            // Log the error and notify clients
            Console.WriteLine($"Error in generating bot response: {ex.Message}");
            message.Append("An error occurred while processing your request.");
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "An error occurred while processing your request.");
        }
        return message.ToString();
    }

    private async Task AppendToCsvLog(string conversationId, string sender, string message)
    {
        // Log messages to a CSV or other logging mechanism here
        // For simplicity, we'll print the log here
        Console.WriteLine($"{DateTime.Now}, {conversationId}, {sender}: {message}");
    }
}
