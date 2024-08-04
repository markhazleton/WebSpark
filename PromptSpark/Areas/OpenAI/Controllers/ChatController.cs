using System.Text;

namespace PromptSpark.Areas.OpenAI.Controllers;



/// <summary>
/// Controller for managing chat functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ChatController"/> class.
/// </remarks>
/// <param name="service">The GPT service.</param>
public class ChatController(IGPTService service,
    IUserPromptService userPromptService,
    IGPTDefinitionTypeService typeService) : OpenAIBaseController
{
    /// <summary>
    /// Displays the index view with a list of all user prompts.
    /// </summary>
    /// <returns>The index view.</returns>
    public async Task<IActionResult> Index()
    {
        var list = await typeService.GetAllGPTDefinitionTypes();
        return View(list);
    }
    public async Task<IActionResult> GetChatDetails(int id)
    {
        var response = await service.FindResponseByUserPromptIdAsync(id);
        return PartialView("_UserPromptDetail", response);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await userPromptService.ReadAsync(0);
        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Create(UserPromptDto model)
    {
        if (ModelState.IsValid)
        {
            var results = await userPromptService.CreateAsync(model);
            if (results != null)
            {
                results = await userPromptService.RefreshDefinitionResponses(results.UserPromptId);
                return RedirectToAction("Details", new { id = results.DefinitionType, UserPromptId = results.UserPromptId });
            }
            return View(results);
        }
        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> Details(string id, int UserPromptId = 0)
    {
        var definitionType = await typeService.GetGPTDefinitionTypeByKey(id);
        if (definitionType == null)
        {
            return NotFound();
        }

        definitionType.CurrentUserPromptId = definitionType
            .Prompts.Where(w => w.UserPromptId == UserPromptId)
            .FirstOrDefault()?.UserPromptId ?? 0;
        if (definitionType.CurrentUserPromptId == 0)
        {
            definitionType.CurrentUserPromptId = definitionType.Prompts.FirstOrDefault()?.UserPromptId ?? 0;
        }
        return View(definitionType);
    }
    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var response = await typeService.FindUserPromptByUserPromptIdAsync(id);
        if (response == null)
        {
            return NotFound();
        }
        var content = response.DefinitionResponses.FirstOrDefault()?.SystemResponse ?? string.Empty;
        // Strip the beginning and ending strings
        if (content.StartsWith("```markdown") && content.EndsWith("```"))
        {
            content = content.Substring(11, content.Length - 14); // Remove '''markdown and '''
        }
        var fileContent = Encoding.UTF8.GetBytes(content);
        return File(fileContent, "text/markdown", $"PromptResponse_{id}.md");
    }
}
