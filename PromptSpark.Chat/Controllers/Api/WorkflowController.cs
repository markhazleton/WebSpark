using Microsoft.AspNetCore.Mvc;
using PromptSpark.Chat.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptSpark.Chat.Controllers.Api;

/// <summary>
/// Controller to manage workflow operations.
/// </summary>
public class WorkflowController : ApiBaseController
{
    private readonly Workflow _workflow;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowController"/> class.
    /// </summary>
    public WorkflowController()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "workflow.json");
        var jsonTemplate = System.IO.File.ReadAllText(jsonPath);
        _workflow = JsonSerializer.Deserialize<Workflow>(jsonTemplate, _jsonOptions) ?? new();
    }

    /// <summary>
    /// Gets the node details by node ID.
    /// </summary>
    /// <param name="nodeId">The ID of the node.</param>
    /// <returns>The node details including question and answers.</returns>
    [HttpGet("{nodeId}")]
    public IActionResult GetNode(string nodeId)
    {
        var node = _workflow.Nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node == null) return NotFound();

        var response = new WorkflowNodeResponse
        {
            Question = node.Question,
            Answers = node.Answers.Select(answer => new AnswerOption
            {
                Response = answer.Response,
                Link = Url.Action(nameof(GetNode), new { nodeId = answer.NextNode })
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Starts the workflow by redirecting to the start node.
    /// </summary>
    /// <returns>Redirection to the start node.</returns>
    [HttpGet("init")]
    public IActionResult StartWorkflow()
    {
        return RedirectToAction(nameof(GetNode), new { nodeId = _workflow.StartNode });
    }

    /// <summary>
    /// Endpoint to say 'thanks' at the end of the workflow.
    /// </summary>
    /// <returns>A workflow response with a thank-you message and no options.</returns>
    [HttpGet("end")]
    public IActionResult SayThanks()
    {
        var response = new WorkflowNodeResponse
        {
            Question = "Thanks for using our workflow service!",
            Answers = [] // No options provided for the end of the workflow
        };

        return Ok(response);
    }

}