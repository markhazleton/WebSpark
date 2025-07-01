using Microsoft.Extensions.Logging;
using TriviaSpark.Domain.Extensions;
using TriviaSpark.Domain.Models;
using TriviaSpark.Domain.Services;
using WebSpark.HttpClientUtility.RequestResult;

namespace TriviaSpark.Domain.OpenTriviaDb;

public class OpenTriviaDbQuestionSource(
    ILogger<OpenTriviaDbQuestionSource> logger,
    IHttpRequestResultService httpGetCallService) : IQuestionSourceAdapter
{

    /// <summary>
    /// Creates a sequence of QuestionModel objects from the specified sequence of Trivia objects.
    /// </summary>
    /// <param name="trivia">The sequence of Trivia objects to create questions from.</param>
    /// <returns>A sequence of QuestionModel objects.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the trivia parameter is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the trivia parameter contains null or empty elements.</exception>
    private IEnumerable<QuestionModel> Create(IEnumerable<Trivia>? trivia)
    {
        if (trivia == null)
        {
            logger.LogError("The trivia parameter cannot contain null elements.");
            throw new ArgumentNullException(nameof(trivia), "The trivia parameter cannot be null.");
        }

        if (trivia.Any(t => t == null))
        {
            logger.LogError("The trivia parameter cannot contain null elements.");
            throw new ArgumentException("The trivia parameter cannot contain null elements.", nameof(trivia));
        }
        return trivia.Select(t => Create(t));
    }

    private static QuestionModel Create(Trivia trivia)
    {
        QuestionModel questionModel = new()
        {
            QuestionId = trivia.question.GetDeterministicHashCode().ToString(),
            Category = trivia.category,
            Difficulty = trivia.difficulty.ParseEnum<Difficulty>(),
            QuestionText = trivia.question,
            Type = trivia.type.ParseEnum<QuestionType>(),
            Source = "OpenTriviaDb"
        };
        questionModel.AddAnswer(trivia.correct_answer, true);
        foreach (var answer in trivia.incorrect_answers)
        {
            questionModel.AddAnswer(answer, false);
        }
        return questionModel;
    }

    public async Task<List<QuestionModel>> GetQuestions(
        int questionCount = 1,
        Difficulty difficulty = Difficulty.Easy,
        CancellationToken ct = default)
    {
        var questionList = new List<QuestionModel>();
        ;
        var results = new HttpRequestResult<OpenTBbResponse>
        {
            RequestPath = $"https://opentdb.com/api.php?amount={questionCount}&difficulty={difficulty.ToString().ToLower()}&type=multiple"
        };
        results = await httpGetCallService.HttpSendRequestResultAsync<OpenTBbResponse>(results, ct: ct).ConfigureAwait(false);
        if (results?.ResponseResults?.results is null)
        {
            logger.LogError("No results returned from OpenTriviaDb");
        }
        questionList.AddRange(Create(results?.ResponseResults?.results));
        return questionList;
    }
}
