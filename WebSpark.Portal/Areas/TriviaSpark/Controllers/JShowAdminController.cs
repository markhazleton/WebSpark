using global::TriviaSpark.JShow.Models;
using global::TriviaSpark.JShow.Service;
using HttpClientUtility.MemoryCache;
namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

public class JShowAdminController(

    IMemoryCacheManager memoryCacheManager,
    IJShowService jShowService) : TriviaSparkBaseController(memoryCacheManager, jShowService)
{

    // JShow CRUD Operations
    public async Task<IActionResult> Index()
    {
        var shows = await _jShowService.GetJShowsAsync();
        return View(shows);
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
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _jShowService.DeleteJShowAsync(id);
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
    public async Task<IActionResult> EditQuestion(string id, QuestionVM question)
    {
        if (id != question.Id)
        {
            return NotFound();
        }

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
