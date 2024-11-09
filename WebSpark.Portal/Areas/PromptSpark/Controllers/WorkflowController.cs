using PromptSpark.Domain.PromptSparkChat;

namespace WebSpark.Portal.Areas.PromptSpark.Controllers;

public class WorkflowController(IWorkflowService _workflowService) : PromptSparkBaseController
{
    public IActionResult AddNode(string fileName)
    {
        ViewBag.FileName = fileName; // Pass the fileName to the view for reference
        return View(new Node());
    }

    [HttpPost]
    public IActionResult AddNode(Node newNode, string fileName)
    {
        var workflow = _workflowService.LoadWorkflow(fileName);
        _workflowService.AddNode(newNode);
        _workflowService.SaveWorkflow(workflow);
        return RedirectToAction("Details", new { fileName });  // Redirect to the Details view for the current workflow
    }

    public IActionResult DeleteNode(string id, string fileName)
    {
        var workflow = _workflowService.LoadWorkflow(fileName);
        _workflowService.DeleteNode(id);
        _workflowService.SaveWorkflow(workflow);
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

    [HttpPost]
    public IActionResult EditNode(EditNodeViewModel updatedNode)
    {
        var workflow = _workflowService.LoadWorkflow(updatedNode.FileName);

        var node = workflow.Nodes.FirstOrDefault(n => n.Id == updatedNode.Id);
        if (node == null)
        {
            return NotFound();
        }

        // Update node properties
        node.Question = updatedNode.Question;

        _workflowService.UpdateNode(node);    // Update the node in-memory
        _workflowService.SaveWorkflow(workflow); // Save the workflow file

        return RedirectToAction("Details", new { fileName = updatedNode.FileName });
    }

    public IActionResult EditNode(string id, string fileName)
    {
        var workflow = _workflowService.LoadWorkflow(fileName);
        var node = workflow.Nodes.FirstOrDefault(n => n.Id == id);

        if (node == null)
        {
            return NotFound();
        }
        var viewModel = new EditNodeViewModel(node, fileName);
        return View(viewModel);
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
