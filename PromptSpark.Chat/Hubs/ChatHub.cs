using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptSpark.Chat.PromptFlow;

namespace PromptSpark.Chat.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ConversationService _conversationService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        ILogger<ChatHub> logger,
        ConversationService conversationService,
        IChatService chatService)
    {
        _logger = logger;
        _conversationService = conversationService;
        _chatService = chatService;
    }

    private static ChatHistory BuildChatHistoryFromConversation(Conversation conversation)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are in a conversation, keep your answers brief, always ask follow-up questions, ask if ready for full answer.");
        foreach (var chatEntry in conversation.ChatHistory)
        {
            if (!string.IsNullOrEmpty(chatEntry.UserMessage))
                chatHistory.AddUserMessage(chatEntry.UserMessage);
            if (!string.IsNullOrEmpty(chatEntry.BotResponse))
                chatHistory.AddSystemMessage(chatEntry.BotResponse);
        }
        return chatHistory;
    }

    public async Task ProgressWorkflow(string conversationId, string userResponse)
    {
        var conversation = _conversationService.Lookup(conversationId);
        if (conversation == null)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "Error: Conversation not found.");
            return;
        }

        try
        {
            var currentNode = conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
            if (currentNode == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "Error in workflow progression. Returning to the current node.");
                return;
            }
            var timestamp = DateTime.Now;
            await Clients.All.SendAsync("ReceiveMessage", conversation.UserName ?? "User", userResponse, conversationId);

            _conversationService.AddChatEntry(conversation, conversation.UserName ?? "User", userResponse, timestamp);

            if (userResponse != null)
            {
                var nextNodeId = currentNode.Answers
                    ?.FirstOrDefault(a => a.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase))
                    ?.NextNode;
                if (nextNodeId != null)
                {
                    conversation.CurrentNodeId = nextNodeId;
                    currentNode = conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
                }
                else
                {
                    var chatHistory = BuildChatHistoryFromConversation(conversation);
                    chatHistory.AddUserMessage(userResponse);

                    await _chatService.EngageChatAgent(chatHistory, conversationId, Clients.Caller);

                    return;
                }
            }

            var adaptiveCardJson = _conversationService.GenerateAdaptiveCardJson(currentNode);
            _logger.LogInformation("AdaptiveCard being sent: {AdaptiveCardJson}", adaptiveCardJson);

            await Clients.Caller.SendAsync("ReceiveAdaptiveCard", adaptiveCardJson);
        }
        catch (Exception ex)
        {
            _conversationService.HandleWorkflowError(ex, conversation);
        }
    }

    public async Task SendMessage(string message, string conversationId)
    {
        var conversation = _conversationService.Lookup(conversationId);
        var user = conversation.UserName ?? "User";
        var timestamp = DateTime.Now;
        await Clients.All.SendAsync("ReceiveMessage", user, message, conversationId);
        _conversationService.AddChatEntry(conversation, user, message, timestamp);
        var chatHistory = BuildChatHistoryFromConversation(conversation);
        chatHistory.AddUserMessage(message);
        var botResponse = await _chatService.GenerateBotResponse(chatHistory);
        _conversationService.AddChatEntry(conversation, "PromptSpark", message, timestamp, botResponse);
    }

    public Task SetUserName(string conversationId, string userName)
    {
        var conversation = _conversationService.Lookup(conversationId);

        // Update existing conversation with new username and reset to start node if needed
        conversation.UserName = userName;
        conversation.CurrentNodeId = conversation.CurrentNodeId == conversation.Workflow.StartNode ? conversation.Workflow.StartNode : conversation.Workflow.StartNode;

        // Save or update the conversation in the dictionary service
        _conversationService.Save(conversationId, conversation);

        return Task.CompletedTask;
    }

    public async Task StartWorkflow(string conversationId)
    {
        await ProgressWorkflow(conversationId, null);
    }
}
