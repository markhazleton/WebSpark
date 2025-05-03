using global::TriviaSpark.JShow.Models;
using global::TriviaSpark.JShow.Service;
using System.Text.Json;
using WebSpark.HttpClientUtility.MemoryCache;
using WebSpark.Portal.Areas.TriviaSpark.Models.JShow;
namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

public class JShowAdminController(
    IMemoryCacheManager memoryCacheManager,
    IJShowService jShowService) : TriviaSparkBaseController(memoryCacheManager, jShowService)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    // JShow CRUD Operations
    public async Task<IActionResult> Index()
    {
        var shows = await _jShowService.GetJShowsAsync();
        return View(shows);
    }
    [HttpGet]
    public IActionResult ValidateJson()
    {
        return View(new JsonInputModel());
    }
    [HttpPost]
    public async Task<IActionResult> ValidateJson(JsonInputModel input)
    {
        if (string.IsNullOrEmpty(input.JsonData))
        {
            TempData["ValidationResult"] = "No JSON data provided.";
            TempData["AlertClass"] = "danger"; // Set the alert class to display error
            return RedirectToAction(nameof(ValidateJson));
        }

        try
        {
            // Deserialize the JSON string into JShowVM
            var jShow = JsonSerializer.Deserialize<JShowVM>(input.JsonData, _jsonOptions);

            if (jShow == null)
            {
                TempData["ValidationResult"] = "Invalid JSON format.";
                TempData["AlertClass"] = "danger";
                return RedirectToAction(nameof(ValidateJson));
            }

            var jShowVM = _jShowService.LoadByJsonString(input.JsonData);
            if (jShowVM == null)
            {
                TempData["ValidationResult"] = "Unable to create J-Show. JSON was a valid string.";
                TempData["AlertClass"] = "danger";
                return RedirectToAction(nameof(ValidateJson));
            }

            // Validate the deserialized object
            if (!TryValidateModel(jShowVM))
            {
                // If validation fails, return the validation errors
                TempData["ValidationResult"] = "Validation Failed.";
                TempData["AlertClass"] = "danger";
                return RedirectToAction(nameof(ValidateJson));
            }

            var dbJShow = await GetJShowList(jShowVM);

            // If the JShowVM object is valid, return a success message
            TempData["ValidationResult"] = "The JShow JSON is valid.";
            TempData["AlertClass"] = "success";
            return RedirectToAction(nameof(ValidateJson));
        }
        catch (JsonException ex)
        {
            TempData["ValidationResult"] = $"Error deserializing JSON: {ex.Message}";
            TempData["AlertClass"] = "danger";
            return RedirectToAction(nameof(ValidateJson));
        }
        catch (Exception ex)
        {
            TempData["ValidationResult"] = $"Unexpected error: {ex.Message}";
            TempData["AlertClass"] = "danger";
            return RedirectToAction(nameof(ValidateJson));
        }
    }
    // Action to get a flattened list of questions
    public async Task<IActionResult> AllQuestions(int showNumber)
    {
        var shows = await GetJShowList();
        var questionViewModels = GetFlattenedQuestions(shows);
        return View(questionViewModels);
    }

    private List<QuestionVM> GetFlattenedQuestions(List<JShowVM> shows)
    {
        var questionViewModels = new List<QuestionVM>();

        foreach (var show in shows)
        {

            void AddQuestionsFromRound(string roundName, RoundVM round)
            {
                foreach (var category in round.Categories)
                {
                    foreach (var question in category.Questions)
                    {
                        questionViewModels.Add(new QuestionVM
                        {
                            Id = question.Id,
                            QuestionText = question.QuestionText,
                            Answer = question.Answer,
                            Value = question.Value,
                            ShowNumber = show.ShowNumber,
                            Theme = show.Theme,
                            AirDate = show.AirDate,
                            RoundName = roundName,
                            CategoryName = category.Name
                        });
                    }
                }
            }
            AddQuestionsFromRound("Round One", show.Rounds.Jeopardy);
            AddQuestionsFromRound("Round Two", show.Rounds.DoubleJeopardy);
            if (show.Rounds.FinalJeopardy != null)
            {
                questionViewModels.Add(new QuestionVM
                {
                    Id = Guid.NewGuid().ToString(),
                    QuestionText = show.Rounds.FinalJeopardy.QuestionText,
                    Answer = show.Rounds.FinalJeopardy.Answer,
                    Value = 0, // No specific value for Final Jeopardy
                    ShowNumber = show.ShowNumber,
                    Theme = show.Theme,
                    AirDate = show.AirDate,
                    RoundName = "Final Round",
                    CategoryName = show.Rounds.FinalJeopardy.Category
                });
            }
        }
        return questionViewModels;
    }

    public IActionResult Create()
    {
        return View(new JShowVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JShowVM jshow)
    {
        if (ModelState.IsValid)
        {
            await _jShowService.CreateJShowAsync(jshow);
            return RedirectToAction(nameof(Index));
        }
        return View(jshow);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var show = await _jShowService.GetJShowByIdAsync(id);
        if (show == null)
        {
            return NotFound();
        }
        return View(show);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, JShowVM jshow)
    {
        if (id != jshow.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _jShowService.UpdateJShowAsync(jshow);
            return RedirectToAction(nameof(Index));
        }
        return View(jshow);
    }

    public async Task<IActionResult> Details(string id)
    {
        var show = await _jShowService.GetJShowByIdAsync(id);
        if (show == null)
        {
            return NotFound();
        }
        return View(show);
    }

    public async Task<IActionResult> Delete(string id)
    {
        var show = await _jShowService.GetJShowByIdAsync(id);
        if (show == null)
        {
            return NotFound();
        }
        return View(show);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(JShowVM showToDelete)
    {
        if (showToDelete == null)
        {
            return NotFound();
        }

        await _jShowService.DeleteJShowAsync(showToDelete.Id);
        return RedirectToAction(nameof(Index));
    }

    // Round CRUD Operations
    public async Task<IActionResult> CreateRound(string jshowId)
    {
        var round = new RoundVM { JShowId = jshowId };
        return View(round);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRound(RoundVM round)
    {
        if (ModelState.IsValid)
        {
            await _jShowService.CreateRoundAsync(round); // Implement CreateRoundAsync in service
            return RedirectToAction(nameof(Edit), new { id = round.JShowId });
        }
        return View(round);
    }

    public async Task<IActionResult> EditRound(string id)
    {
        var round = await _jShowService.GetRoundByIdAsync(id); // Implement GetRoundByIdAsync in service
        if (round == null)
        {
            return NotFound();
        }
        return View(round);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRound(string id, RoundVM round)
    {
        if (id != round.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _jShowService.UpdateRoundAsync(round); // Implement UpdateRoundAsync in service
            return RedirectToAction(nameof(Edit), new { id = round.JShowId });
        }
        return View(round);
    }

    public async Task<IActionResult> DeleteRound(string id)
    {
        var round = await _jShowService.GetRoundByIdAsync(id);
        if (round == null)
        {
            return NotFound();
        }
        return View(round);
    }

    [HttpPost, ActionName("DeleteRound")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoundConfirmed(string id, string jshowId)
    {
        await _jShowService.DeleteRoundAsync(id); // Implement DeleteRoundAsync in service
        return RedirectToAction(nameof(Edit), new { id = jshowId });
    }

    // Category CRUD Operations
    public async Task<IActionResult> CreateCategory(string roundId)
    {
        var category = new CategoryVM { RoundId = roundId };
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryVM category)
    {
        if (ModelState.IsValid)
        {
            await _jShowService.CreateCategoryAsync(category); // Implement CreateCategoryAsync in service
            return RedirectToAction(nameof(EditRound), new { id = category.RoundId });
        }
        return View(category);
    }

    public async Task<IActionResult> EditCategory(string id)
    {
        var category = await _jShowService.GetCategoryByIdAsync(id); // Implement GetCategoryByIdAsync in service
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(string id, CategoryVM category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _jShowService.UpdateCategoryAsync(category); // Implement UpdateCategoryAsync in service
            return RedirectToAction(nameof(EditRound), new { id = category.RoundId });
        }
        return View(category);
    }

    public async Task<IActionResult> DeleteCategory(string id)
    {
        var category = await _jShowService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost, ActionName("DeleteCategory")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryConfirmed(string id, string roundId)
    {
        await _jShowService.DeleteCategoryAsync(id); // Implement DeleteCategoryAsync in service
        return RedirectToAction(nameof(EditRound), new { id = roundId });
    }

    // Question CRUD Operations
    public async Task<IActionResult> CreateQuestion(string categoryId)
    {
        var category = await _jShowService.GetCategoryByIdAsync(categoryId);

        var question = new QuestionVM { CategoryName = category.Name };
        return View(question);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(QuestionVM question)
    {
        if (ModelState.IsValid)
        {
            await _jShowService.CreateQuestionAsync(question); // Implement CreateQuestionAsync in service
            return RedirectToAction(nameof(EditCategory), new { id = question.CategoryId });
        }
        return View(question);
    }

    public async Task<IActionResult> EditQuestion(string id)
    {
        var question = await _jShowService.GetQuestionByIdAsync(id); // Implement GetQuestionByIdAsync in service
        if (question == null)
        {
            return NotFound();
        }
        return View(question);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(QuestionVM question)
    {

        if (ModelState.IsValid)
        {
            await _jShowService.UpdateQuestionAsync(question); // Implement UpdateQuestionAsync in service
            return RedirectToAction(nameof(EditCategory), new { id = question.CategoryId });
        }
        return View(question);
    }

    public async Task<IActionResult> DeleteQuestion(string id)
    {
        var question = await _jShowService.GetQuestionByIdAsync(id);
        if (question == null)
        {
            return NotFound();
        }
        return View(question);
    }

    [HttpPost, ActionName("DeleteQuestion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestionConfirmed(string id, string categoryId)
    {
        await _jShowService.DeleteQuestionAsync(id); // Implement DeleteQuestionAsync in service
        return RedirectToAction(nameof(EditCategory), new { id = categoryId });
    }
}
