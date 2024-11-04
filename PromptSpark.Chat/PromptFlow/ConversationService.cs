using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace PromptSpark.Chat.PromptFlow;

public class ConversationService : ConcurrentDictionaryService<Conversation>
{
    private const string STR_ChatBotName = "PromptSpark";

    private readonly IChatService _chatService;
    private readonly ILogger<ConversationService> _logger;
    private readonly IWorkflowService _workflowService;

    public ConversationService(IWorkflowService workflowService, IChatService chatService, ILogger<ConversationService> logger)
    {
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _logger = logger;
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


    public async Task EngageChatAgent(ChatHistory chatHistory, string conversationId, IClientProxy clients, CancellationToken cancellationToken)
    {
        await _chatService.EngageChatAgent(chatHistory, conversationId, clients, cancellationToken);
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
    public async Task<string> GenerateBotResponse(ChatHistory chatHistory, CancellationToken cancellationToken)
    {
        return await _chatService.GenerateBotResponse(chatHistory);
    }

    public Node? GetCurrentNode(Conversation conversation)
    {
        return conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
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
    public async Task<(MessageType messageType, object messageData)?> ProcessSendMessage(
        string message,
        string conversationId,
        Conversation conversation,
        CancellationToken ct)
    {
        var user = conversation.UserName ?? STR_ChatBotName;
        var timestamp = DateTime.Now;

        // Add the message to the conversation history.
        AddChatEntry(conversation, user, message, timestamp);

        // Build the chat history and generate a bot response.
        var chatHistory = BuildChatHistoryFromConversation(conversation);
        chatHistory.AddUserMessage(message);
        var botResponse = await GenerateBotResponse(chatHistory, ct);

        // Add the bot response to the conversation history.
        AddChatEntry(conversation, STR_ChatBotName, message, timestamp, botResponse);

        // Prepare the tuple result with message type and data for SendAsync.
        return (MessageType.ReceiveMessage, new { User = user, Message = message, ConversationId = conversationId });
    }

    public (MessageType messageType, object messageData)? ProcessUserResponse(
        string conversationId,
        string userResponse,
        Conversation conversation,
        CancellationToken ct)
    {
        var currentNode = GetCurrentNode(conversation);
        if (currentNode == null)
        {
            return (MessageType.ReceiveMessage, new { sender = STR_ChatBotName, content = "Error in workflow progression." });
        }

        var adaptiveCardJson = GenerateAdaptiveCardJson(currentNode);

        if (!string.IsNullOrWhiteSpace(userResponse))
        {
            AddChatEntry(conversation, conversation.UserName ?? STR_ChatBotName, userResponse, DateTime.Now, currentNode.Question);
            var matchingAnswer = currentNode?.Answers.FirstOrDefault(answer => answer.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase));

            if (matchingAnswer is null)
            {
                var chatHistory = BuildChatHistoryFromConversation(conversation);
                chatHistory.AddUserMessage(userResponse);

                // Return EngageChatAgent if no matching answer is found.
                return (MessageType.EngageChatAgent, new { chatHistory, conversationId });
            }

            var nextNode = ProgressWorkflow(conversation, userResponse);
            if (nextNode == null)
            {
                return (MessageType.ReceiveMessage, new { sender = STR_ChatBotName, content = "Error in workflow progression." });
            }

            adaptiveCardJson = GenerateAdaptiveCardJson(nextNode);
        }

        _logger.LogInformation("AdaptiveCard being sent: {AdaptiveCardJson}", adaptiveCardJson);
        return (MessageType.ReceiveAdaptiveCard, adaptiveCardJson);
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
}