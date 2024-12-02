using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
namespace PromptSpark.Chat.Services;

public interface IChatService
{
    Task<string> GenerateBotResponse(ChatHistory chatHistory);
    Task EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients, CancellationToken cancellationToken);
}

public class ChatService : IChatService
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IChatCompletionService chatCompletionService, ILogger<ChatService> logger)
    {
        _chatCompletionService = chatCompletionService;
        _logger = logger;
    }

    public async Task<string> GenerateBotResponse(ChatHistory chatHistory)
    {
        var response = new StringBuilder();

        try
        {
            await foreach (var content in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                if (content?.Content != null)
                {
                    response.Append(content.Content);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bot response.");
            return "An error occurred while generating a response.";
        }

        return response.ToString();
    }

    public async Task EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients, CancellationToken cancellationToken)
    {
        if (chatHistory == null) throw new ArgumentNullException(nameof(chatHistory));
        if (clients == null) throw new ArgumentNullException(nameof(clients));
        if (string.IsNullOrEmpty(conversationId)) throw new ArgumentException("Conversation ID cannot be null or empty.", nameof(conversationId));

        try
        {
            await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory).WithCancellation(cancellationToken))
            {
                if (response?.Content != null)
                {
                    await clients.SendAsync("ReceiveMessage", "PromptSpark", response.Content, conversationId, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("EngageChatAgent operation was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error engaging chat agent.");
            await clients.SendAsync("ReceiveMessage", "PromptSpark", "An error occurred while processing your request.");
        }
    }
}
