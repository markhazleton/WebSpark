using PromptSpark.Domain.PromptSparkChat;

namespace WebSpark.Portal.Areas.PromptSpark.Controllers;

public class HomeController(IWorkflowService workflowService) : PromptSparkBaseController
{
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult LearnMore()
    {
        return View();
    }

    public IActionResult KitchenSink()
    {
        return View();
    }
    public IActionResult Chat()
    {
        return View();
    }
    // API endpoint to get workflow data as JSON
    [HttpGet]
    public IActionResult GetWorkflowData(string fileName)
    {
        var workflow = workflowService.LoadWorkflow(fileName);
        return Json(workflow);
    }
    // API endpoint to return list of string with all available workflows
    [HttpGet]
    public IActionResult Workflows()
    {
        var workflows = workflowService.ListAvailableWorkflows();
        return Json(workflows);
    }
}
