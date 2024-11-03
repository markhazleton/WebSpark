using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace PromptSpark.Chat.PromptFlow;

public class ConversationService : ConcurrentDictionaryService<Conversation>
{
    private readonly ILogger<ConversationService> _logger;
    private readonly IWorkflowService _workflowService;
    private readonly IChatService _chatService;

    public ConversationService(IWorkflowService workflowService, IChatService chatService, ILogger<ConversationService> logger)
    {
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _logger = logger;
    }

    public async Task EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients, CancellationToken cancellationToken)
    { 
        await _chatService.EngageChatAgent(chatHistory, conversationId, clients, cancellationToken);
    }
    public async Task<string> GenerateBotResponse(ChatHistory chatHistory, CancellationToken cancellationToken)
    {
        return await _chatService.GenerateBotResponse(chatHistory);
    }

    public ChatHistory BuildChatHistoryFromConversation(Conversation conversation)
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



    public void AddChatEntry(Conversation conversation, string user, string message, DateTime timestamp, string botResponse = "")
    {
        conversation.ChatHistory.Add(new ChatEntry
        {
            Timestamp = timestamp,
            User = user,
            UserMessage = message,
            BotResponse = botResponse
        });
    }

    public string GenerateAdaptiveCardJson(Node currentNode)
    {
        var adaptiveCard = new Dictionary<string, object>
    {
        { "type", "AdaptiveCard" },
        { "version", "1.3" },
        { "body", new object[]
            {
                new Dictionary<string, object>
                {
                    { "type", "TextBlock" },
                    { "text", currentNode?.Question ?? "No question provided." },
                    { "wrap", true },
                    { "size", "Medium" },
                    { "weight", "Bolder" }
                },
                new Dictionary<string, object>
                {
                    { "type", "TextBlock" },
                    { "text", "Select an option below:" },
                    { "wrap", true },
                    { "separator", true }
                },
                new Dictionary<string, object>
                {
                    { "type", "ActionSet" },
                    { "actions", currentNode?.Answers?.Select(answer => new Dictionary<string, object>
                        {
                            { "type", "Action.Submit" },
                            { "title", answer.Response },
                            { "data", new { option = answer.Response } }
                        }).ToArray() ?? Array.Empty<object>()
                    }
                }
            }
        },
        { "$schema", "http://adaptivecards.io/schemas/adaptive-card.json" }
    };

        return JsonSerializer.Serialize(adaptiveCard);
    }

    public void HandleWorkflowError(Exception ex, Conversation conversation)
    {
        _logger.LogError(ex, "Error in ProgressWorkflow for conversation {ConversationId}. Returning the current node.", conversation.ConversationId);
    }

    public override Conversation Lookup(string conversationId)
    {
        return GetOrAdd(conversationId, id =>
        {
            var workflow = _workflowService.LoadWorkflow();
            return new Conversation(workflow, id, null);
        });
    }

    public Node? ProgressWorkflow(Conversation conversation, string userResponse)
    {
        var currentNode = conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
        if (currentNode == null)
        {
            _logger.LogError("Node with ID '{CurrentNodeId}' not found.", conversation.CurrentNodeId);
            return null;
        }
        var selectedAnswer = currentNode.Answers.FirstOrDefault(a => a.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase));
        conversation.CurrentNodeId = selectedAnswer?.NextNode ?? conversation.CurrentNodeId;
        return conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
    }

    public Node? GetCurrentNode(Conversation conversation)
    {
        return conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
    }
}