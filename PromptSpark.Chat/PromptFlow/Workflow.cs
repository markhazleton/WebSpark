using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptSpark.Chat.PromptFlow;


public interface IWorkflowService
{
    Workflow LoadWorkflow();
}
public class WorkflowOptions
{
    public string FilePath { get; set; } = "wwwroot/workflow.json";
}

public class WorkflowService : IWorkflowService
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WorkflowOptions _options;

    public WorkflowService(IOptions<WorkflowOptions> options, JsonSerializerOptions jsonOptions)
    {
        _options = options.Value;
        _jsonOptions = jsonOptions;
    }

    public Workflow LoadWorkflow()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), _options.FilePath);
        var jsonTemplate = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<Workflow>(jsonTemplate, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to load Workflow configuration.");
    }
}

public class Conversation
{
    public Conversation()
    {

    }
    public Conversation(Workflow workflow, string conversationId, string? userName)
    {
        UserName = userName ?? "Anonymous";
        ConversationId = conversationId;
        Workflow = workflow;
        CurrentNodeId = workflow.StartNode;
    }

    public List<ChatEntry> ChatHistory { get; set; } = [];
    public string ConversationId { get; set; }
    public string CurrentNodeId { get; set; }
    public string PromptName { get; set; } = "helpful";
    public DateTime StartDate { get; set; } = DateTime.Now;
    public string UserName { get; set; }
    public Workflow Workflow { get; set; }
}

public class ChatEntry
{
    public string BotResponse { get; set; }
    public DateTime Timestamp { get; set; }
    public string User { get; set; }
    public string UserMessage { get; set; }
}


public class Workflow
{

    [JsonPropertyName("nodes")]
    public List<Node> Nodes { get; set; }

    [JsonPropertyName("startNode")]
    public string StartNode { get; set; }
    [JsonPropertyName("workflowId")]
    public string WorkflowId { get; set; }
}

public class Node
{

    [JsonPropertyName("answers")]
    public List<Answer> Answers { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("question")]
    public string Question { get; set; }
}

public class Answer
{

    [JsonPropertyName("nextNode")]
    public string NextNode { get; set; }
    [JsonPropertyName("response")]
    public string Response { get; set; }
}
public class OptionResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; }
}

public class WorkflowNodeResponse
{

    [JsonPropertyName("answers")]
    public List<AnswerOption> Answers { get; set; }
    [JsonPropertyName("question")]
    public string Question { get; set; }
}

public class AnswerOption
{

    [JsonPropertyName("link")]
    public string Link { get; set; }
    [JsonPropertyName("response")]
    public string Response { get; set; }
}

public class ConversationService : ConcurrentDictionaryService<Conversation>
{
    private readonly ILogger<ConversationService> _logger;
    private readonly IWorkflowService _workflowService;

    public ConversationService(IWorkflowService workflowService, ILogger<ConversationService> logger)
    {
        _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
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
        _logger.LogError(ex, $"Error in ProgressWorkflow for conversation {conversation.ConversationId}. Returning the current node.");
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
            _logger.LogError($"Node with ID '{conversation.CurrentNodeId}' not found.");
            return null;
        }

        var selectedAnswer = currentNode.Answers.FirstOrDefault(a => a.Response.Equals(userResponse, StringComparison.OrdinalIgnoreCase));
        conversation.CurrentNodeId = selectedAnswer?.NextNode ?? conversation.CurrentNodeId;

        return conversation.Workflow.Nodes.FirstOrDefault(node => node.Id == conversation.CurrentNodeId);
    }
}