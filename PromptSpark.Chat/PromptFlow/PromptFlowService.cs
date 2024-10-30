using System.Text.Json;
using System.Text.Json.Serialization;
namespace PromptSpark.Chat.PromptFlow;

public class WorkflowService
{
    private readonly Workflow _workflow;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowService"/> class.
    /// </summary>
    public WorkflowService()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "workflow.json");
        var jsonTemplate = File.ReadAllText(jsonPath);
        _workflow = JsonSerializer.Deserialize<Workflow>(jsonTemplate, _jsonOptions);
    }

    /// <summary>
    /// Retrieves a workflow node by its ID.
    /// </summary>
    /// <param name="nodeId">The ID of the node.</param>
    /// <returns>The node details including question and answers, or null if not found.</returns>
    public WorkflowNodeResponse GetNode(string nodeId)
    {
        var node = _workflow.Nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node == null) return null;

        return new WorkflowNodeResponse
        {
            Question = node.Question,
            Answers = node.Answers.Select(answer => new AnswerOption
            {
                Response = answer.Response,
                Link = null // Controller can fill this in
            }).ToList()
        };
    }

    /// <summary>
    /// Gets the start node ID of the workflow.
    /// </summary>
    /// <returns>The start node ID.</returns>
    public string GetStartNode()
    {
        return _workflow.StartNode;
    }
}

