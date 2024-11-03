using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptSpark.Chat.PromptFlow;
using System.Text.Json;

namespace PromptSpark.Chat.Hubs;

public class ChatHub : Hub
{
    private readonly ConcurrentDictionaryService<Conversation> _ConversationDictionary;
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;
    private readonly Workflow _workflow;

    public ChatHub(
        ILogger<ChatHub> logger,
        Workflow workflow,
        ConcurrentDictionaryService<Conversation> conversationDictionary,
        IChatService chatService)
    {
        _logger = logger;
        _workflow = workflow;
        _ConversationDictionary = conversationDictionary;
        _chatService = chatService;
    }

    private Task BroadcastUserMessage(string conversationId, string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message, conversationId);
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

    private static ChatEntry CreateChatEntry(string user, string message, DateTime timestamp, string botResponse)
    {
        return new ChatEntry
        {
            Timestamp = timestamp,
            User = user,
            UserMessage = message,
            BotResponse = botResponse
        };
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

    public async Task ProgressWorkflow(string conversationId, string userResponse)
    {
        var conversation = _ConversationDictionary.Lookup(conversationId);
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

                    await _chatService.EngageChatAgent(chatHistory, conversationId, Clients.Caller);

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

    public async Task SendMessage(string message, string conversationId)
    {
        var conversation = _ConversationDictionary.Lookup(conversationId) ?? new Conversation(_workflow, conversationId, null);

        var user = conversation.UserName ?? "User";
        var timestamp = DateTime.Now;

        await BroadcastUserMessage(conversationId, user, message);

        conversation.ChatHistory.Add(CreateChatEntry(user, message, timestamp, string.Empty));

        var chatHistory = BuildChatHistoryFromConversation(conversation);
        chatHistory.AddUserMessage(message);

        var botResponse = await _chatService.GenerateBotResponse(chatHistory);

        conversation.ChatHistory.Add(CreateChatEntry("PromptSpark", message, timestamp, botResponse));
    }

    public Task SetUserName(string conversationId, string userName)
    {
        var conversation = _ConversationDictionary.Lookup(conversationId) ?? new Conversation(_workflow, conversationId, userName);

        // Update existing conversation with new username and reset to start node if needed
        conversation.UserName = userName;
        conversation.CurrentNodeId = _workflow.StartNode;

        // Save or update the conversation in the dictionary service
        _ConversationDictionary.Save(conversationId, conversation);

        return Task.CompletedTask;
    }

    public async Task StartWorkflow(string conversationId)
    {
        await ProgressWorkflow(conversationId, null);
    }
}
