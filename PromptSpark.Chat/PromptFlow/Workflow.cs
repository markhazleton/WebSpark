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
