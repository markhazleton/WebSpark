using Microsoft.AspNetCore.SignalR;

namespace PromptSpark.Chat.Services;

public class PromptSparkHub : Hub
{
    private const string STR_ChatBotName = "PromptSpark";
    private readonly IHubContext<PromptSparkHub> _hubContext;
    private readonly ConversationService _conversationService;
    private readonly ILogger<PromptSparkHub> _logger;

    public PromptSparkHub(
        IHubContext<PromptSparkHub> hubContext,
        ConversationService conversationService,
        ILogger<PromptSparkHub> logger)
    {
        _logger = logger;
        _hubContext = hubContext;
        _conversationService = conversationService;
    }

    public async Task SendMessage(string conversationId, string message)
    {
        CancellationToken ct = Context.ConnectionAborted;
        var conversation = _conversationService.Lookup(conversationId);
        try
        {
            var sendArgument = await _conversationService.ProcessUserResponse(conversationId, message, conversation, Clients.Caller, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during workflow progression for conversation {ConversationId}", conversationId);
            await Clients.Caller.SendAsync(MessageType.ReceiveMessage.ToString(), STR_ChatBotName, "An error occurred while processing your request.");
            _conversationService.HandleWorkflowError(ex, conversation);
        }
    }

    public Task SetUserName(string conversationId, string userName, string workflowName)
    {
        var conversation = _conversationService.Lookup(conversationId);
        conversation.UserName = userName;
        conversation.Workflow = _conversationService.LoadWorkflow(workflowName);
        if (conversation.CurrentNodeId != conversation.Workflow.StartNode)
        {
            conversation.CurrentNodeId = conversation.Workflow.StartNode;
        }
        _conversationService.Save(conversationId, conversation);
        return Task.CompletedTask;
    }
}
