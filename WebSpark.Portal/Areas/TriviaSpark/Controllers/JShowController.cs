using HttpClientUtility.MemoryCache;
using TriviaSpark.JShow.Models;
using TriviaSpark.JShow.Service;

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
    [Route("JShow/{id}")]
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
}




