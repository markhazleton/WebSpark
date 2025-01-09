using global::TriviaSpark.JShow.Models;
using global::TriviaSpark.JShow.Service;
using HttpClientUtility.MemoryCache;
using System.Text.Json;
using WebSpark.Portal.Areas.TriviaSpark.Models.JShow;
namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

public class JShowJsonController(
    IMemoryCacheManager memoryCacheManager,
    IJShowService jShowService) : TriviaSparkBaseController(memoryCacheManager, jShowService)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public async Task<IActionResult> Index()
    {
        var model = new JsonInputModel();
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> Index(JsonInputModel input)
    {
        if (string.IsNullOrEmpty(input.JsonData))
        {
            TempData["ValidationResult"] = "No JSON data provided.";
            TempData["AlertClass"] = "danger"; // Set the alert class to display error
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Deserialize the JSON string into JShowVM
            var jShow = JsonSerializer.Deserialize<JShowVM>(input.JsonData, _jsonOptions);

            if (jShow == null)
            {
                TempData["ValidationResult"] = "Invalid JSON format.";
                TempData["AlertClass"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var jShowVM = _jShowService.LoadByJsonString(input.JsonData);
            if (jShowVM == null)
            {
                TempData["ValidationResult"] = "Unable to create J-Show. JSON was a valid string.";
                TempData["AlertClass"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            // Validate the deserialized object
            if (!TryValidateModel(jShowVM))
            {
                // If validation fails, return the validation errors
                TempData["ValidationResult"] = "Validation Failed.";
                TempData["AlertClass"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var dbJShow = await GetJShowList(jShowVM);

            // If the JShowVM object is valid, return a success message
            TempData["ValidationResult"] = "The JShow JSON is valid.";
            TempData["AlertClass"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (JsonException ex)
        {
            TempData["ValidationResult"] = $"Error deserializing JSON: {ex.Message}";
            TempData["AlertClass"] = "danger";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ValidationResult"] = $"Unexpected error: {ex.Message}";
            TempData["AlertClass"] = "danger";
            return RedirectToAction(nameof(Index));
        }
    }
}
