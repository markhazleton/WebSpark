using Microsoft.AspNetCore.SignalR;
using PromptSpark.Chat.PromptFlow;

namespace PromptSpark.Chat.Hubs;

public class ChatHub : Hub
{
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
            var sendArgument = ProcessUserResponse(conversationId, userResponse, conversation, ct);
            if (sendArgument.HasValue)
            {
                if (sendArgument.Value.messageType == "EngageChatAgent")
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
            await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "An error occurred while processing your request.");
            _conversationService.HandleWorkflowError(ex, conversation);
        }
    }

    private (string messageType, object messageData)? ProcessUserResponse(
    string conversationId,
    string userResponse,
    Conversation conversation,
    CancellationToken ct)
    {
        var currentNode = _conversationService.GetCurrentNode(conversation);
        if (currentNode == null)
        {
            return ("ReceiveMessage", new { sender = "PromptSpark", content = "Error in workflow progression." });
        }

        var adaptiveCardJson = _conversationService.GenerateAdaptiveCardJson(currentNode);

        if (!string.IsNullOrWhiteSpace(userResponse))
        {
            _conversationService.AddChatEntry(conversation, conversation.UserName ?? "User", userResponse, DateTime.Now, currentNode.Question);
            var matchingAnswer = currentNode?.Answers.FirstOrDefault(answer => answer.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase));

            if (matchingAnswer is null)
            {
                var chatHistory = _conversationService.BuildChatHistoryFromConversation(conversation);
                chatHistory.AddUserMessage(userResponse);

                // This indicates that you may want to handle chat agent engagement separately
                // since it's asynchronous. You could return a marker here or split this logic out.
                return ("EngageChatAgent", new { chatHistory, conversationId });
            }

            var nextNode = _conversationService.ProgressWorkflow(conversation, userResponse);
            if (nextNode == null)
            {
                return ("ReceiveMessage", new { sender = "PromptSpark", content = "Error in workflow progression." });
            }

            adaptiveCardJson = _conversationService.GenerateAdaptiveCardJson(nextNode);
        }
        _logger.LogInformation("AdaptiveCard being sent: {AdaptiveCardJson}", adaptiveCardJson);
        return ("ReceiveAdaptiveCard", adaptiveCardJson);
    }





    public async Task SendMessage(string message, string conversationId)
    {
        CancellationToken ct = CancellationToken.None;
        var conversation = _conversationService.Lookup(conversationId);
        var user = conversation.UserName ?? "User";
        var timestamp = DateTime.Now;
        await Clients.All.SendAsync("ReceiveMessage", user, message, conversationId);
        _conversationService.AddChatEntry(conversation, user, message, timestamp);
        var chatHistory = _conversationService.BuildChatHistoryFromConversation(conversation);
        chatHistory.AddUserMessage(message);
        var botResponse = await _conversationService.GenerateBotResponse(chatHistory, ct);
        _conversationService.AddChatEntry(conversation, "PromptSpark", message, timestamp, botResponse);
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
