using Microsoft.AspNetCore.Mvc;
using PromptSpark.Domain.Service;

namespace WebSpark.Admin.Areas.Prompt.Controllers;

/// <summary>
/// Controller for managing chat functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ChatController"/> class.
/// </remarks>
/// <param name="service">The GPT service.</param>
public class ChatController(IGPTService service,
    IGPTDefinitionTypeService typeService) : PromptBaseController
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
