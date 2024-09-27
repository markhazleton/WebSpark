using HttpClientUtility.MemoryCache;
using WebSpark.Portal.Areas.TriviaSpark.Models.JShow;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

public class JShowController(
    IMemoryCacheManager memoryCacheManager,
    IJShowService jShowService) : TriviaSparkBaseController
{
    private readonly IJShowService _jShowService = jShowService;

    public IActionResult Index()
    {
        // Fetch the list of available shows
        var shows = _jShowService.GetJShows();

        // If no shows are available, return an empty view
        if (shows == null || !shows.Any())
        {
            return View("Index", new List<JShow>());
        }

        // Store the shows list in cache for quick access
        memoryCacheManager.Set("JShowList", shows, 30);

        return View("Index", shows);
    }
    [Route("JShow/{id}")]
    public IActionResult JShow(string id)
    {
        // Retrieve the list of shows from cache
        var shows = memoryCacheManager.Get("JShowList", () =>
        {
            return _jShowService.GetJShows() ?? [];
        });

        // Find the selected show by show number
        var selectedShow = shows.FirstOrDefault(s => s.Theme.Equals(id, StringComparison.CurrentCultureIgnoreCase));

        if (selectedShow == null)
        {
            return NotFound("Show not found.");
        }

        // Store the selected show in cache for reuse
        memoryCacheManager.Set("JShow", selectedShow, 30);

        return View("JShow", selectedShow);
    }

    public IActionResult Reveal(string id, string showid)
    {
        var shows = memoryCacheManager.Get<List<JShow>>("JShowList", () =>
        {
            return [];
        });
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


    // Action to get a flattened list of questions
    public IActionResult AllQuestions(int showNumber)
    {

        var shows = memoryCacheManager.Get("JShowList", () =>
        {
            return _jShowService.GetJShows() ?? [];
        });

        var questionViewModels = GetFlattenedQuestions(shows);

        return View(questionViewModels);
    }

    private List<QuestionViewModel> GetFlattenedQuestions(List<JShow> shows)
    {
        var questionViewModels = new List<QuestionViewModel>();

        foreach (var show in shows)
        {

            void AddQuestionsFromRound(string roundName, Round round)
            {
                foreach (var category in round.Categories)
                {
                    foreach (var question in category.Questions)
                    {
                        questionViewModels.Add(new QuestionViewModel
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
                questionViewModels.Add(new QuestionViewModel
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
}




