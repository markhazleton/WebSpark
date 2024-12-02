using Microsoft.AspNetCore.Mvc;
using PromptSpark.Chat.Services;

namespace PromptSpark.Chat.Controllers;

public class WorkflowAdminController : Controller
{
    private readonly IWorkflowService _workflowService;

    public WorkflowAdminController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public IActionResult ExportJson(string fileName)
    {
        var workflow = _workflowService.LoadWorkflow(fileName);

        if (workflow == null)
        {
            return NotFound();
        }
        // Return the workflow as JSON
        return Json(workflow);
    }


    public IActionResult AddNode(string fileName)
    {
        var workflow = _workflowService.LoadWorkflow(fileName);
        if (workflow == null)
        {
            return NotFound();
        }
        var addNode = new EditNodeViewModel()
        {
            FileName = fileName
        };
        return View(addNode);
    }

    [HttpPost]
    public IActionResult AddNode(EditNodeViewModel newNode)
    {
        var workflow = _workflowService.LoadWorkflow(newNode.FileName);
        _workflowService.AddNode(newNode);
        _workflowService.SaveWorkflow(workflow);
        return RedirectToAction("Details", new { newNode.FileName });  // Redirect to the Details view for the current workflow
    }

    public IActionResult DeleteNode(string id, string fileName)
    {
        var node = _workflowService.LoadNode(id, fileName);
        if (node == null)
        {
            return NotFound();
        }
        _workflowService.DeleteNode(node);
        return RedirectToAction("Details", new { fileName });  // Redirect to Details after deletion
    }

    // Display details of a specific workflow
    public IActionResult Details(string fileName)
    {
        try
        {
            var workflow = _workflowService.LoadWorkflow(fileName);
            return View(workflow);  // This view will show details for the selected workflow
        }
        catch (FileNotFoundException)
        {
            return NotFound("Workflow file not found.");
        }
    }

    public IActionResult EditNode(string id, string fileName)
    {
        var node = _workflowService.LoadNode(id, fileName);
        if (node == null)
        {
            return NotFound();
        }
        return View(node);
    }

    [HttpPost]
    public IActionResult EditNode(EditNodeViewModel updatedNode)
    {
        var workflow = _workflowService.LoadWorkflow(updatedNode.FileName);
        var node = workflow.Nodes.FirstOrDefault(n => n.Id == updatedNode.Id);
        if (node == null)
        {
            return NotFound();
        }
        _workflowService.UpdateNode(updatedNode);    // Update the node in-memory
        return RedirectToAction("Details", new { fileName = updatedNode.FileName });
    }

    // View to display the flowchart
    public IActionResult Flowchart(string fileName)
    {
        return View("Flowchart", fileName);
    }

    // API endpoint to get workflow data as JSON
    [HttpGet]
    public IActionResult GetWorkflowData(string fileName)
    {
        var workflow = _workflowService.LoadWorkflow(fileName);
        return Json(workflow);
    }

    // List all available workflows
    public IActionResult Index()
    {
        var workflows = _workflowService.ListAvailableWorkflows();
        return View(workflows);  // This view will list all workflow files
    }

    // Display a tree view for the workflow
    public IActionResult Tree(string fileName)
    {
        var workflow = _workflowService.LoadWorkflow(fileName);
        return View(workflow);
    }

    // API endpoint to return list of string with all available workflows
    [HttpGet]
    public IActionResult Workflows()
    {
        var workflows = _workflowService.ListAvailableWorkflows();
        return Json(workflows);
    }
}


