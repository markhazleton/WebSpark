using Microsoft.AspNetCore.SignalR;
using PromptSpark.Chat.PromptFlow;

namespace PromptSpark.Chat.Hubs;

public class ChatHub : Hub
{
    private const string STR_EngageChatAgent = "EngageChatAgent";
    private const string STR_ReceiveMessage = "ReceiveMessage";
    private const string STR_ChatBotName = "PromptSpark";
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ConversationService _conversationService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
         IHubContext<ChatHub> hubContext,
        ConversationService conversationService,
        ILogger<ChatHub> logger)
    {
        _logger = logger;
        _hubContext = hubContext;
        _conversationService = conversationService;
    }
    public async Task ProgressWorkflow(string conversationId, string userResponse)
    {
        var conversation = _conversationService.Lookup(conversationId);
        CancellationToken ct = CancellationToken.None;
        try
        {
            var sendArgument = _conversationService.ProcessUserResponse(conversationId, userResponse, conversation, ct);
            if (sendArgument.HasValue)
            {
                if (sendArgument.Value.messageType == STR_EngageChatAgent)
                {
                    var chatHistory = _conversationService.BuildChatHistoryFromConversation(conversation);
                    await _conversationService.EngageChatAgent(chatHistory, conversationId, Clients.Caller, ct);
                }
                else
                {
                    await Clients.Caller.SendAsync(sendArgument.Value.messageType, sendArgument.Value.messageData);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during workflow progression for conversation {ConversationId}", conversationId);
            await Clients.Caller.SendAsync(STR_ReceiveMessage, STR_ChatBotName, "An error occurred while processing your request.");
            _conversationService.HandleWorkflowError(ex, conversation);
        }
    }

    public async Task SendMessage(string message, string conversationId)
    {
        CancellationToken ct = CancellationToken.None;
        var conversation = _conversationService.Lookup(conversationId);
        var sendArgument = await _conversationService.ProcessSendMessage(message, conversationId, conversation, ct);
        if (sendArgument.HasValue)
        {
            if (sendArgument.Value.messageType == STR_EngageChatAgent)
            {
                var chatHistory = _conversationService.BuildChatHistoryFromConversation(conversation);
                await _conversationService.EngageChatAgent(chatHistory, conversationId, Clients.Caller, ct);
            }
            else
            {
                await Clients.Caller.SendAsync(sendArgument.Value.messageType, sendArgument.Value.messageData);
            }
        }
    }

    public Task SetUserName(string conversationId, string userName)
    {
        var conversation = _conversationService.Lookup(conversationId);
        conversation.UserName = userName;
        if (conversation.CurrentNodeId != conversation.Workflow.StartNode)
        {
            conversation.CurrentNodeId = conversation.Workflow.StartNode;
        }
        // Save or update the conversation in the dictionary service
        _conversationService.Save(conversationId, conversation);

        return Task.CompletedTask;
    }
}
