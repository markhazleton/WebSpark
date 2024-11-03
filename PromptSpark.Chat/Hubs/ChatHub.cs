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
        IChatService chatService,
        ConversationService conversationService,
        ILogger<ChatHub> logger)
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

        try
        {
            var currentNode = _conversationService.GetCurrentNode(conversation);
            if (currentNode == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "Error in workflow progression.");
                return;
            }
            var adaptiveCardJson = _conversationService.GenerateAdaptiveCardJson(currentNode);

            if (userResponse != null)
            {
                _conversationService.AddChatEntry(conversation, conversation.UserName ?? "User", userResponse, DateTime.Now, currentNode.Question);
                var matchingAnswer = currentNode?.Answers.FirstOrDefault(answer => answer.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase));
                if (matchingAnswer is null)
                {
                    var chatHistory = BuildChatHistoryFromConversation(conversation);
                    chatHistory.AddUserMessage(userResponse);
                    await _chatService.EngageChatAgent(chatHistory, conversationId, Clients.Caller);
                    return;
                }

                var nextNode = _conversationService.ProgressWorkflow(conversation, userResponse);
                if (nextNode == null)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "Error in workflow progression.");
                    return;
                }
                adaptiveCardJson = _conversationService.GenerateAdaptiveCardJson(nextNode);
            }

            _logger.LogInformation("AdaptiveCard being sent: {AdaptiveCardJson}", adaptiveCardJson);
            await Clients.Caller.SendAsync("ReceiveAdaptiveCard", adaptiveCardJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during workflow progression for conversation {ConversationId}", conversationId);
            await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "An error occurred while processing your request.");
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
        conversation.UserName = userName;
        if (conversation.CurrentNodeId != conversation.Workflow.StartNode)
        {
            conversation.CurrentNodeId = conversation.Workflow.StartNode;
        }
        // Save or update the conversation in the dictionary service
        _conversationService.Save(conversationId, conversation);

        return Task.CompletedTask;
    }

    public async Task StartWorkflow(string conversationId)
    {
        await ProgressWorkflow(conversationId, null);
    }
}
