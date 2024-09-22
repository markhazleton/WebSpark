using PromptSpark.Domain.Models;
using PromptSpark.Domain.Service;

namespace WebSpark.Portal.Areas.PromptSpark.Controllers;

/// <summary>
/// Controller for managing chat functionality.
/// </summary>
public class ExplorerController(
    IGPTService service,
    IUserPromptService userPromptService,
    IGPTDefinitionTypeService typeService,
    ILogger<ExplorerController> logger) : PromptSparkBaseController
{
    /// <summary>
    /// Displays the index view with a list of all user prompts.
    /// </summary>
    /// <returns>The index view.</returns>
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("Entering Index method.");
        try
        {
            var list = await typeService.GetAllGPTDefinitionTypes();
            logger.LogInformation("Retrieved {Count} GPT Definition Types.", list?.Count ?? 0);
            return View(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving GPT Definition Types.");
            return View("Error"); // Or redirect to a more user-friendly error page.
        }
    }

    public async Task<IActionResult> GetChatDetails(int id)
    {
        logger.LogInformation("Entering GetChatDetails method with UserPromptId: {UserPromptId}.", id);
        try
        {
            var response = await service.FindResponseByUserPromptIdAsync(id);
            if (response == null)
            {
                logger.LogWarning("No chat details found for UserPromptId: {UserPromptId}.", id);
                return NotFound();
            }

            logger.LogInformation("Chat details retrieved successfully for UserPromptId: {UserPromptId}.", id);
            return PartialView("_UserPromptDetail", response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving chat details for UserPromptId: {UserPromptId}.", id);
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        logger.LogInformation("Entering Create (GET) method.");
        try
        {
            var model = await userPromptService.ReadAsync(0);
            logger.LogInformation("Read initial UserPrompt model successfully.");
            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while preparing the Create view.");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserPromptDto model)
    {
        logger.LogInformation("Entering Create (POST) method with model: {Model}.", model);
        if (!ModelState.IsValid)
        {
            logger.LogWarning("ModelState is invalid in Create (POST) method.");
            return View(model);
        }

        try
        {
            var results = await userPromptService.CreateAsync(model);
            if (results != null)
            {
                logger.LogInformation("UserPrompt created successfully with UserPromptId: {UserPromptId}.", results.UserPromptId);
                results = await userPromptService.RefreshDefinitionResponses(results.UserPromptId);
                logger.LogInformation("UserPrompt responses refreshed successfully.");
                return RedirectToAction("Details", new { id = results.DefinitionType, UserPromptId = results.UserPromptId });
            }

            logger.LogWarning("Failed to create UserPrompt. Results returned null.");
            return View(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating a UserPrompt.");
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, int UserPromptId = 0)
    {
        logger.LogInformation("Entering Details method with DefinitionType: {DefinitionType}, UserPromptId: {UserPromptId}.", id, UserPromptId);
        try
        {
            var definitionType = await typeService.GetGPTDefinitionTypeByKey(id);
            if (definitionType == null)
            {
                logger.LogWarning("DefinitionType not found for key: {Key}.", id);
                return NotFound();
            }

            definitionType.CurrentUserPromptId = definitionType
                .Prompts.Where(w => w.UserPromptId == UserPromptId)
                .FirstOrDefault()?.UserPromptId ?? 0;

            logger.LogInformation("Details retrieved successfully for DefinitionType: {DefinitionType}.", id);
            return View(definitionType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving details for DefinitionType: {DefinitionType}.", id);
            return View("Error");
        }
    }
}
