using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptSpark.Chat.PromptFlow;
using System.Text;
using System.Text.Json;

namespace PromptSpark.Chat.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly Workflow _workflow;
    private readonly ConcurrentDictionaryService<Conversation> _chatHistoryService;

    public ChatHub(
        ILogger<ChatHub> logger,
        IChatCompletionService chatCompletionService,
        Workflow workflow,
        ConcurrentDictionaryService<Conversation> chatHistoryService)
    {
        _logger = logger;
        _chatCompletionService = chatCompletionService;
        _workflow = workflow;
        _chatHistoryService = chatHistoryService;
    }

    public Task SetUserName(string conversationId, string userName)
    {
        var conversation = _chatHistoryService.Lookup(conversationId) ?? new Conversation
        {
            UserName = userName,
            Workflow = _workflow,
            ConversationId = conversationId,
            CurrentNodeId = _workflow.StartNode // Initialize the starting node
        };

        // Update existing conversation with new username and reset to start node if needed
        conversation.UserName = userName;
        conversation.CurrentNodeId = _workflow.StartNode;

        // Save or update the conversation in the dictionary service
        _chatHistoryService.Save(conversationId, conversation);

        return Task.CompletedTask;
    }

    public async Task SendMessage(string message, string conversationId)
    {
        var conversation = GetOrCreateConversation(conversationId);

        var user = conversation.UserName ?? "User";
        var timestamp = DateTime.Now;

        await BroadcastUserMessage(conversationId, user, message);

        conversation.ChatHistory.Add(CreateChatEntry(user, message, timestamp, string.Empty));

        var chatHistory = BuildChatHistoryFromConversation(conversation);
        chatHistory.AddUserMessage(message);

        var botResponse = await GenerateStreamingBotResponse(chatHistory, conversationId);

        conversation.ChatHistory.Add(CreateChatEntry("PromptSpark", message, timestamp, botResponse));
    }

    public async Task StartWorkflow(string conversationId)
    {
        await ProgressWorkflow(conversationId, null);
    }

    public async Task ProgressWorkflow(string conversationId, string userResponse)
    {
        var conversation = _chatHistoryService.Lookup(conversationId);
        if (conversation == null)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "Error: Conversation not found.");
            return;
        }

        try
        {
            var currentNode = GetCurrentNode(conversation);
            if (currentNode == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "Error in workflow progression. Returning to the current node.");
                return;
            }

            var timestamp = DateTime.Now;
            await BroadcastUserMessage(conversationId, conversation.UserName ?? "User", userResponse);

            conversation.ChatHistory.Add(CreateChatEntry(conversation.UserName ?? "User", userResponse, timestamp, currentNode.Question));

            if (userResponse != null)
            {
                var nextNodeId = GetNextNodeId(currentNode, userResponse);
                if (nextNodeId != null)
                {
                    conversation.CurrentNodeId = nextNodeId;
                    currentNode = GetCurrentNode(conversation);
                }
                else
                {
                    var chatHistory = BuildChatHistoryFromConversation(conversation);
                    chatHistory.AddUserMessage(userResponse);

                    await EngageChatAgent(chatHistory, conversationId);
                    return;
                }
            }

            var adaptiveCardJson = GenerateAdaptiveCardJson(currentNode);
            _logger.LogInformation("AdaptiveCard being sent: {AdaptiveCardJson}", adaptiveCardJson);

            await Clients.Caller.SendAsync("ReceiveAdaptiveCard", adaptiveCardJson);
        }
        catch (Exception ex)
        {
            HandleWorkflowError(ex, conversationId, conversation);
        }
    }

    public async Task EngageChatAgent(ChatHistory chatHistory, string conversationId)
    {
        try
        {
            await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                if (response?.Content != null)
                {
                    await Clients.All.SendAsync("ReceiveMessage", "PromptSpark", response.Content, conversationId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error engaging chat agent.");
            await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "An error occurred while processing your request.");
        }
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

                    if (response.Content.Contains('\n'))
                    {
                        var contentToSend = buffer.ToString();
                        await Clients.All.SendAsync("ReceiveMessage", "PromptSpark", contentToSend, conversationId);
                        _logger.LogDebug("{conversationId}: Sent content: {contentToSend}", conversationId, contentToSend);

                        message.Append(contentToSend);
                        buffer.Clear();
                    }
                }
            }

            if (buffer.Length > 0)
            {
                var remainingContent = buffer.ToString();
                await Clients.All.SendAsync("ReceiveMessage", "PromptSpark", remainingContent, conversationId);
                message.Append(remainingContent);
                _logger.LogDebug("{conversationId}: Remaining content: {RemainingContent}", conversationId, remainingContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in generating bot response");
            await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "An error occurred while processing your request.");
            message.Append("An error occurred while processing your request.");
        }

        return message.ToString();
    }

    private string GenerateAdaptiveCardJson(Node currentNode)
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

    private Conversation GetOrCreateConversation(string conversationId)
    {
        return _chatHistoryService.Lookup(conversationId) ?? new Conversation
        {
            Workflow = _workflow,
            ConversationId = conversationId,
            CurrentNodeId = _workflow.StartNode
        };
    }

    private ChatHistory BuildChatHistoryFromConversation(Conversation conversation)
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

    private Node GetCurrentNode(Conversation conversation)
    {
        return conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
    }

    private string GetNextNodeId(Node currentNode, string userResponse)
    {
        return currentNode.Answers
            ?.FirstOrDefault(a => a.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase))
            ?.NextNode;
    }

    private void HandleWorkflowError(Exception ex, string conversationId, Conversation conversation)
    {
        _logger.LogError(ex, "Error in ProgressWorkflow. Returning the current node.");

        var currentNode = GetCurrentNode(conversation);

        var responsePackage = new
        {
            Question = currentNode?.Question ?? "An error occurred, please try again.",
            Options = currentNode?.Answers?.Select(answer => new OptionResponse
            {
                Response = answer.Response
            }).ToList() ?? new List<OptionResponse>()
        };

        Clients.Caller.SendAsync("ReceiveMessagePackage", responsePackage);
    }

    private ChatEntry CreateChatEntry(string user, string message, DateTime timestamp, string botResponse)
    {
        return new ChatEntry
        {
            Timestamp = timestamp,
            User = user,
            UserMessage = message,
            BotResponse = botResponse
        };
    }

    private Task BroadcastUserMessage(string conversationId, string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message, conversationId);
    }
}
