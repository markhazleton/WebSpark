using Microsoft.AspNetCore.Mvc;
using PromptSpark.Chat.PromptFlow;
using System.Text.Json;

namespace PromptSpark.Chat.Controllers;

public class WorkflowAdminController : Controller
{
    private readonly IWorkflowService _workflowService;

    public WorkflowAdminController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }
    public IActionResult Tree()
    {
        var workflow = _workflowService.LoadWorkflow();
        return View(workflow);
    }
    public IActionResult Sankey()
    {
        // Get the Sankey data from the service
        var sankeyData = _workflowService.GetSankeyData();

        // Serialize it to JSON for passing to the view
        var jsonData = JsonSerializer.Serialize(sankeyData, new JsonSerializerOptions
        {
            WriteIndented = true // Make JSON more readable
        });

        ViewData["SankeyData"] = jsonData;
        return View();
    }

    // API endpoint to get workflow data as JSON
    [HttpGet]
    public IActionResult GetWorkflowData()
    {
        var workflow = _workflowService.LoadWorkflow();
        return Json(workflow);
    }
    // View to display the flowchart
    public IActionResult Flowchart()
    {
        return View();
    }

    public IActionResult Index()
    {
        var workflow = _workflowService.LoadWorkflow();
        return View(workflow);
    }

    public IActionResult EditNode(string id)
    {
        var node = _workflowService.GetNodeById(id);
        if (node == null)
        {
            return NotFound();
        }
        return View(node);
    }

    [HttpPost]
    public IActionResult EditNode(Node updatedNode)
    {
        _workflowService.UpdateNode(updatedNode);
        return RedirectToAction("Index");
    }

    public IActionResult AddNode()
    {
        return View(new Node());
    }

    [HttpPost]
    public IActionResult AddNode(Node newNode)
    {
        _workflowService.AddNode(newNode);
        return RedirectToAction("Index");
    }

    public IActionResult DeleteNode(string id)
    {
        _workflowService.DeleteNode(id);
        return RedirectToAction("Index");
    }
}


