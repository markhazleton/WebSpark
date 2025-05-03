using TriviaSpark.JShow.Models;
using TriviaSpark.JShow.Service;
using WebSpark.HttpClientUtility.MemoryCache;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

public class JShowMainController(
    IMemoryCacheManager memoryCacheManager,
    IJShowService jShowService) : TriviaSparkBaseController(memoryCacheManager, jShowService)
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var shows = await GetJShowList();
        if (shows == null || shows.Count == 0)
        {
            return View("Index", new List<JShowVM>());
        }
        return View("Index", shows);
    }

    [HttpGet]
    [Route("/TriviaSpark/JShow/{id}")]
    public async Task<IActionResult> JShow(string id)
    {
        var shows = await GetJShowList();
        // Find the selected show by show number
        var selectedShow = shows.FirstOrDefault(s => s.Theme.Equals(id, StringComparison.CurrentCultureIgnoreCase));

        if (selectedShow == null)
        {
            return NotFound("Show not found.");
        }
        return View("JShow", selectedShow);
    }
    [HttpGet]
    [Route("/TriviaSpark/JShow/Reveal")]
    public async Task<IActionResult> Reveal(string id, string showid)
    {
        var shows = await GetJShowList();

        var show = shows.FirstOrDefault(s => s.Id == showid);
        if (show == null)
        {
            return NotFound("Show not found.");
        }
        var question = show.GetQuestionByID(id);
        if (question == null)
        {
            return NotFound("Question not found.");
        }
        return PartialView("_Reveal", question);
    }




}







