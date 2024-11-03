using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
namespace PromptSpark.Chat.PromptFlow;

public interface IChatService
{
    Task<string> GenerateBotResponse(ChatHistory chatHistory);
    Task EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients);
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

        await foreach (var content in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            if (content?.Content != null)
            {
                response.Append(content.Content);
            }
        }

        return response.ToString();
    }

    public async Task EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients)
    {
        try
        {
            await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                if (response?.Content != null)
                {
                    await clients.SendAsync("ReceiveMessage", "PromptSpark", response.Content, conversationId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error engaging chat agent.");
            await clients.SendAsync("ReceiveMessage", "PromptSpark", "An error occurred while processing your request.");
        }
    }
}

