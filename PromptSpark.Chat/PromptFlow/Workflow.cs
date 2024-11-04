using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptSpark.Chat.PromptFlow;
public enum MessageType
{
    EngageChatAgent,
    ReceiveAdaptiveCard,
    ReceiveMessage
}

public interface IWorkflowService
{
    Workflow LoadWorkflow();
    Node GetNodeById(string nodeId);
    void AddNode(Node newNode);
    void UpdateNode(Node updatedNode);
    void DeleteNode(string nodeId);
    void SaveWorkflow(Workflow workflow);
    object GetSankeyData();
}
public class WorkflowOptions
{
    public string FilePath { get; set; } = "wwwroot/workflow.json";
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
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), _options.FilePath);
        var jsonTemplate = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<Workflow>(jsonTemplate, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to load Workflow configuration.");
    }
    public Node GetNodeById(string nodeId)
    {
        return _workflow.Nodes.Find(n => n.Id == nodeId);
    }

    public void AddNode(Node newNode)
    {
        if (_workflow.Nodes.Any(n => n.Id == newNode.Id))
        {
            throw new InvalidOperationException($"Node with ID {newNode.Id} already exists.");
        }

        _workflow.Nodes.Add(newNode);
        SaveWorkflow(_workflow);
    }

    public void UpdateNode(Node updatedNode)
    {
        var existingNodeIndex = _workflow.Nodes.FindIndex(n => n.Id == updatedNode.Id);
        if (existingNodeIndex == -1)
        {
            throw new InvalidOperationException($"Node with ID {updatedNode.Id} does not exist.");
        }

        _workflow.Nodes[existingNodeIndex] = updatedNode;
        SaveWorkflow(_workflow);
    }

    public void DeleteNode(string nodeId)
    {
        var nodeToRemove = _workflow.Nodes.Find(n => n.Id == nodeId);
        if (nodeToRemove == null)
        {
            throw new InvalidOperationException($"Node with ID {nodeId} does not exist.");
        }

        _workflow.Nodes.Remove(nodeToRemove);
        UpdateReferencesAfterNodeDeletion(nodeId);
        SaveWorkflow(_workflow);
    }

    public void SaveWorkflow(Workflow workflow)
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), _options.FilePath);
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

    public object GetSankeyData()
    {
        // Step 1: Collect all nodes and create a map from node ID to index
        var nodes = _workflow.Nodes.Select(n => new { id = n.Id, name = n.Question }).ToList();
        var nodeIndexMap = nodes.Select((node, index) => new { node.id, index })
                                .ToDictionary(x => x.id, x => x.index);

        // Step 2: Helper function to detect cycles using Depth-First Search (DFS)
        bool IsCircularLink(string sourceId, string targetId, HashSet<string> visited)
        {
            if (visited.Contains(targetId)) return true;
            visited.Add(targetId);

            var targetNode = _workflow.Nodes.FirstOrDefault(n => n.Id == targetId);
            if (targetNode != null)
            {
                foreach (var answer in targetNode.Answers)
                {
                    if (!string.IsNullOrEmpty(answer.NextNode) && IsCircularLink(targetId, answer.NextNode, new HashSet<string>(visited)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Step 3: Collect all links, replacing source/target with indices from the map and filtering out circular links
        var links = new List<object>();

        foreach (var node in _workflow.Nodes)
        {
            foreach (var answer in node.Answers)
            {
                if (!string.IsNullOrEmpty(answer.NextNode) && nodeIndexMap.ContainsKey(answer.NextNode))
                {
                    // Check if this link is circular
                    if (!IsCircularLink(node.Id, answer.NextNode, new HashSet<string> { node.Id }))
                    {
                        links.Add(new
                        {
                            source = nodeIndexMap[node.Id],      // Use index for source
                            target = nodeIndexMap[answer.NextNode], // Use index for target
                            value = 1 // Default weight for each link
                        });
                    }
                }
            }
        }

        return new { nodes, links };
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
