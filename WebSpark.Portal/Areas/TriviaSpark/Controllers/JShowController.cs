using HttpClientUtility.MemoryCache;
using System.Text.Json;
using TriviaSpark.JShow.Models;
using TriviaSpark.JShow.Service;
using WebSpark.Portal.Areas.TriviaSpark.Models.JShow;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

public class JShowController(
    IMemoryCacheManager memoryCacheManager,
    IJShowService jShowService) : TriviaSparkBaseController(memoryCacheManager, jShowService)
{

    public async Task<IActionResult> Index()
    {
        var shows = await GetJShowList();
        if (shows == null || shows.Count == 0)
        {
            return View("Index", new List<JShowVM>());
        }
        return View("Index", shows);
    }
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







