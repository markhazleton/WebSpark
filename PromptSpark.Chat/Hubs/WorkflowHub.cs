using Microsoft.AspNetCore.SignalR;
using PromptSpark.Chat.PromptFlow;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PromptSpark.Chat.Hubs;

public class WorkflowHub : Hub
{
    // Track each client's workflow state
    private static readonly ConcurrentDictionary<string, string> UserWorkflowStates = new ConcurrentDictionary<string, string>();
    private readonly Workflow _workflow;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


    public WorkflowHub()
    {
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "workflow.json");
        var jsonTemplate = File.ReadAllText(jsonPath);
        _workflow = JsonSerializer.Deserialize<Workflow>(jsonTemplate, _jsonOptions) ?? new Workflow();
    }

    // Method to start the workflow
    public async Task StartWorkflow()
    {
        var startNode = _workflow.Nodes.FirstOrDefault(n => n.Id == _workflow.StartNode);
        if (startNode != null)
        {
            // Store the user's current node state
            UserWorkflowStates[Context.ConnectionId] = _workflow.StartNode;

            // Send the initial question to the client
            await Clients.Caller.SendAsync("ReceiveNode", new WorkflowNodeResponse
            {
                Question = startNode.Question,
                Answers = startNode.Answers.Select(a => new AnswerOption
                {
                    Response = a.Response,
                    Link = a.NextNode // Store only the ID; we'll handle lookup on next step
                }).ToList()
            });
        }
    }

    // Method for handling responses from clients
    public async Task RespondToWorkflow(string response)
    {
        // Retrieve the user's current node state
        if (UserWorkflowStates.TryGetValue(Context.ConnectionId, out var currentNodeId))
        {
            var currentNode = _workflow.Nodes.FirstOrDefault(n => n.Id == currentNodeId);
            var selectedAnswer = currentNode?.Answers.FirstOrDefault(a => a.Response.Equals(response, StringComparison.OrdinalIgnoreCase));

            if (selectedAnswer != null)
            {
                // Update the user's current node state
                UserWorkflowStates[Context.ConnectionId] = selectedAnswer.NextNode;

                var nextNode = _workflow.Nodes.FirstOrDefault(n => n.Id == selectedAnswer.NextNode);

                if (nextNode != null)
                {
                    // Send the next question to the client
                    await Clients.Caller.SendAsync("ReceiveNode", new WorkflowNodeResponse
                    {
                        Question = nextNode.Question,
                        Answers = nextNode.Answers.Select(a => new AnswerOption
                        {
                            Response = a.Response,
                            Link = a.NextNode
                        }).ToList()
                    });
                }
                else
                {
                    // End of workflow
                    await Clients.Caller.SendAsync("ReceiveNode", new WorkflowNodeResponse
                    {
                        Question = "Thanks for using our workflow service!",
                        Answers = [] // No options at the end
                    });
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Invalid response.");
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", "Workflow state not found. Please restart.");
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Clean up the user's workflow state on disconnect
        UserWorkflowStates.TryRemove(Context.ConnectionId, out _);
        await base.OnDisconnectedAsync(exception);
    }
}
