using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptSpark.Domain.PromptSparkChat;
public enum MessageType
{
    EngageChatAgent,
    ReceiveAdaptiveCard,
    ReceiveMessage
}
public class FlexibleStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt32().ToString();
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        throw new JsonException("Unexpected token type for Id property.");
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
public interface IWorkflowService
{
    Workflow LoadWorkflow();
    Workflow LoadWorkflow(string fileName);
    Node? GetNodeById(string nodeId);
    void AddNode(Node newNode);
    void UpdateNode(Node updatedNode);
    void DeleteNode(string nodeId);
    void SaveWorkflow(Workflow workflow);
    List<string> ListAvailableWorkflows();
}

public class WorkflowOptions
{
    public string FilePath { get; set; } = "wwwroot/workflow.json";  // Default workflow file
    public string DirectoryPath { get; set; } = "wwwroot/workflows";  // Directory for workflow files
}

public class WorkflowService : IWorkflowService
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly WorkflowOptions _options;
    private Workflow _workflow;

    public WorkflowService(IOptions<WorkflowOptions> options, JsonSerializerOptions jsonOptions)
    {
        _options = options.Value;
        _jsonOptions = jsonOptions;
        _workflow = LoadWorkflow();
    }

    public Workflow LoadWorkflow()
    {
        return LoadWorkflow("workflow.json");
    }

    public Workflow LoadWorkflow(string fileName)
    {
        var filePath = fileName ?? "workflow.json";
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), _options.DirectoryPath, filePath);

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Workflow file '{filePath}' not found in '{_options.DirectoryPath}'");

        var jsonTemplate = File.ReadAllText(jsonPath);
        var workflow = JsonSerializer.Deserialize<Workflow>(jsonTemplate, _jsonOptions)
                      ?? throw new InvalidOperationException("Failed to load Workflow configuration.");

        workflow.WorkFlowName = Path.GetFileNameWithoutExtension(filePath);
        workflow.WorkFlowFileName = filePath;
        return workflow;
    }

    public List<string> ListAvailableWorkflows()
    {
        var list = new List<string>();
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), _options.DirectoryPath);

        if (!Directory.Exists(directoryPath))
            return list;

        foreach (var file in Directory.GetFiles(directoryPath, "*.json"))
        {
            list.Add(Path.GetFileName(file));
        }

        return list;
    }

    public Node? GetNodeById(string nodeId)
    {
        return _workflow.Nodes.Find(n => n.Id == nodeId);
    }

    public void AddNode(Node newNode)
    {
        if (_workflow.Nodes.Any(n => n.Id == newNode.Id))
            throw new InvalidOperationException($"Node with ID {newNode.Id} already exists.");

        _workflow.Nodes.Add(newNode);
        SaveWorkflow(_workflow);
    }

    public void UpdateNode(Node updatedNode)
    {
        var existingNodeIndex = _workflow.Nodes.FindIndex(n => n.Id == updatedNode.Id);
        if (existingNodeIndex == -1)
            throw new InvalidOperationException($"Node with ID {updatedNode.Id} does not exist.");

        _workflow.Nodes[existingNodeIndex] = updatedNode;
        SaveWorkflow(_workflow);
    }

    public void DeleteNode(string nodeId)
    {
        var nodeToRemove = _workflow.Nodes.Find(n => n.Id == nodeId);
        if (nodeToRemove == null)
            throw new InvalidOperationException($"Node with ID {nodeId} does not exist.");

        _workflow.Nodes.Remove(nodeToRemove);
        UpdateReferencesAfterNodeDeletion(nodeId);
        SaveWorkflow(_workflow);
    }

    public void SaveWorkflow(Workflow workflow)
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), _options.DirectoryPath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = string.IsNullOrEmpty(workflow.WorkFlowFileName) ? "workflow.json" : workflow.WorkFlowFileName;
        var jsonPath = Path.Combine(directoryPath, fileName);
        var json = JsonSerializer.Serialize(workflow, _jsonOptions);

        File.WriteAllText(jsonPath, json);
    }

    private void UpdateReferencesAfterNodeDeletion(string deletedNodeId)
    {
        foreach (var node in _workflow.Nodes)
        {
            node.Answers.RemoveAll(answer => answer.NextNode == deletedNodeId);
        }
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
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string StartNode { get; set; }
    [JsonPropertyName("workflowId")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string WorkflowId { get; set; }
    [JsonPropertyName("workflowName")]
    public string WorkFlowName { get; set; } = "workflow";
    public string? WorkFlowFileName { get; internal set; }
}
public class EditNodeViewModel : Node
{
    public EditNodeViewModel()
    {

    }
    public EditNodeViewModel(Node node, string filename)
    {
        Id = node.Id;
        Question = node.Question;
        Answers = node.Answers;
        FileName = filename;
    }

    public string FileName { get; set; }
}
public class Node
{

    [JsonPropertyName("answers")]
    public List<Answer> Answers { get; set; }
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Id { get; set; }

    [JsonPropertyName("question")]
    public string Question { get; set; }
}

public class Answer
{

    [JsonPropertyName("nextNode")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string NextNode { get; set; }
    [JsonPropertyName("response")]
    public string Response { get; set; }
    [JsonPropertyName("system")]
    public string SystemPrompt { get; set; } = "You are a chat agent";
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
