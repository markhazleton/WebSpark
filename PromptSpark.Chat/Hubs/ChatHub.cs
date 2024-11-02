using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.VisualBasic;
using PromptSpark.Chat.PromptFlow;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace PromptSpark.Chat.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, Conversation> ChatHistoryCache = new();
    private readonly ILogger<ChatHub> _logger;
    private readonly IChatCompletionService _chatCompletionService;
    private Workflow _workflow;

    // Constructor to load workflow from JSON
    public ChatHub(ILogger<ChatHub> logger, IChatCompletionService chatCompletionService, string workflowJson)
    {
        _logger = logger;
        _chatCompletionService = chatCompletionService;
        _workflow = JsonSerializer.Deserialize<Workflow>(workflowJson);
    }
    public Task SetUserName(string conversationId, string userName)
    {
        if (!ChatHistoryCache.TryGetValue(conversationId, out var conversation))
        {
            conversation = new Conversation
            {
                UserName = userName,
                Workflow = _workflow,
                ConversationId = conversationId,
                CurrentNodeId = _workflow.StartNode // Initialize the starting node
            };
            ChatHistoryCache[conversationId] = conversation;
        }
        else
        {
            conversation.ConversationId = conversationId;
            conversation.UserName = userName;
            conversation.Workflow = _workflow;
            conversation.CurrentNodeId = _workflow.StartNode; // Reset to start node if needed
        }
        return Task.CompletedTask;
    }


    public async Task SendMessage(string message, string conversationId)
    {
        if (!ChatHistoryCache.TryGetValue(conversationId, out var conversation))
        {
            conversation = new Conversation();
            ChatHistoryCache[conversationId] = conversation;
        }

        // Get user name from the conversation
        var user = conversation.UserName ?? "User";

        var timestamp = DateTime.Now;

        // Broadcast the user's message to all clients
        await Clients.All.SendAsync("ReceiveMessage", user, message, conversationId);

        // Add the user's message to the conversation history
        conversation.ChatHistory.Add(new ChatEntry
        {
            Timestamp = timestamp,
            User = user,
            UserMessage = message,
            BotResponse = string.Empty // Placeholder for bot's response
        });

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("You are in a conversation, keep your answers brief, always ask follow-up questions, ask if ready for full answer.");

        foreach (var chatEntry in conversation.ChatHistory)
        {
            if(chatEntry.UserMessage != null)
                chatHistory.AddUserMessage(chatEntry.UserMessage);
        }

        chatHistory.AddUserMessage(message);

        // Generate bot response with streaming
        var botResponse = await GenerateStreamingBotResponse(chatHistory, conversationId);

        // Add the bot response to the conversation history
        conversation.ChatHistory.Add(new ChatEntry
        {
            Timestamp = timestamp,
            User = "PromptSpark",
            UserMessage = message,
            BotResponse = botResponse
        });
    }


    public async Task StartWorkflow(string conversationId)
    {
        await ProgressWorkflow(conversationId, null);
    }
    public async Task ProgressWorkflow(string conversationId, string userResponse)
    {
        if (!ChatHistoryCache.TryGetValue(conversationId, out var conversation))
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

            // Broadcast the user's message to all clients
            await Clients.All.SendAsync("ReceiveMessage", conversation.UserName ?? "User", userResponse, conversationId);

            // Record the user's message in chat history
            conversation.ChatHistory.Add(new ChatEntry
            {
                Timestamp = timestamp,
                User = conversation.UserName ?? "User",
                UserMessage = userResponse,
                BotResponse = currentNode.Question // Placeholder for bot's response
            });

            if (userResponse != null)
            {
                var nextAnswer = currentNode.Answers.FirstOrDefault(a => a.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase));

                if (nextAnswer != null)
                {
                    conversation.CurrentNodeId = nextAnswer.NextNode;
                    currentNode = conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
                }
                else
                {
                    var chatHistory = new ChatHistory();
                    foreach(var chatEntry in conversation.ChatHistory)
                    {
                        if (chatEntry.UserMessage != null)
                            chatHistory.AddUserMessage(chatEntry.UserMessage);
                        if (chatEntry.BotResponse != null)
                            chatHistory.AddSystemMessage(chatEntry.BotResponse);
                    }
                    chatHistory.AddUserMessage(userResponse);

                    await EngageChatAgent(chatHistory, conversationId);
                    return;
                }
            }

            // Prepare the AdaptiveCard content
            // Prepare the AdaptiveCard content
            var adaptiveCard = new Dictionary<string, object>
{
    { "type", "AdaptiveCard" },
    { "version", "1.3" },
    { "body", new object[]
        {
            new Dictionary<string, object>
            {
                { "type", "TextBlock" },
                { "text", currentNode.Question ?? "No question provided." },
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
                { "actions", currentNode.Answers?.Select(answer => new Dictionary<string, object>
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

            // Convert the AdaptiveCard object to JSON
            var adaptiveCardJson = JsonSerializer.Serialize(adaptiveCard);

            _logger.LogInformation("AdaptiveCard being sent: {AdaptiveCardJson}", adaptiveCardJson);

            // Send the AdaptiveCard JSON to the client
            await Clients.Caller.SendAsync("ReceiveAdaptiveCard", adaptiveCardJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProgressWorkflow. Returning the current node.");
            var currentNode = conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);

            var responsePackage = new
            {
                Question = currentNode?.Question ?? "An error occurred, please try again.",
                Options = currentNode?.Answers?.Select(answer => new OptionResponse
                {
                    Response = answer.Response
                }).ToList() ?? new List<OptionResponse>()
            };

            await Clients.Caller.SendAsync("ReceiveMessagePackage", responsePackage);
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
                        _logger.LogInformation("{conversationId}:Send content: {contentToSend}", conversationId, contentToSend);
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
                _logger.LogInformation("{conversationId}:Remaining content: {RemainingContent}", conversationId, remainingContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in generating bot response");
            message.Append("An error occurred while processing your request.");
            await Clients.Caller.SendAsync("ReceiveMessage", "PromptSpark", "An error occurred while processing your request.");
        }
        return message.ToString();
    }


}
