using PromptSpark.Areas.OpenAI.Service;

namespace PromptSpark.Areas.OpenAI.Controllers;

/// <summary>
/// Controller for managing chat functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ChatController"/> class.
/// </remarks>
/// <param name="service">The GPT service.</param>
public class ChatController(IGPTService service,
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
    public async Task<IActionResult> GetChatDetails(string id)
    {
        var response = await service.FindResponseByUserPromptTextAsync(id);
        return PartialView("_ChatDetailsPartial", response);
    }

    public async Task<IActionResult> Details(string id)
    {
        var definitionType = await typeService.GetGPTDefinitionTypeByKey(id);
        if (definitionType == null)
        {
            return NotFound();
        }
        return View(definitionType);
    }
}
