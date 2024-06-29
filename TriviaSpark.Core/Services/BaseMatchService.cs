using System.Security.Claims;

namespace TriviaSpark.Core.Services;

public abstract class BaseMatchService(Models.MatchModel _match) : IMatchService
{
    protected virtual Models.MatchModel Match => _match ?? throw new InvalidOperationException("Match not set yet");

    protected static Models.MatchModel CreateMatch()
    {
        return new Models.MatchModel()
        {
            UserId = "66eae7b7-c163-4913-8aaf-421a23f0d5d9",
            MatchQuestions = new QuestionProvider(),
            MatchQuestionAnswers = [],
            MatchName = "Trivia Match"
        };
    }
    public virtual Task<Models.MatchModel?> AddAnswerAsync(int MatchId, Models.QuestionAnswerModel currentAnswer, CancellationToken ct = default)
    {
        return Task.FromResult<Models.MatchModel?>(CreateMatch());
    }
    public virtual string GetMatchStatus(Models.MatchModel match)
    {
        match.ScoreCard = match.MatchQuestions.CalculateScore(match.MatchQuestionAnswers);

        var correctQuestions = match.MatchQuestions.GetCorrectQuestions(match.MatchQuestionAnswers);
        return $"{correctQuestions.Count} correct out of {match.MatchQuestions.Count} total in {match.MatchQuestionAnswers.Count()} attempts.";
    }
    public virtual Task<Models.MatchModel?> GetMoreQuestionsAsync(int MatchId, int NumberOfQuestionsToAdd = 1, Models.Difficulty difficulty = Models.Difficulty.Easy, CancellationToken ct = default)
    {
        return Task.FromResult<Models.MatchModel?>(CreateMatch());
    }
    public virtual Models.QuestionModel? GetNextQuestion(Models.MatchModel match)
    {
        return null;
    }
    public virtual Task<List<Models.MatchModel>> GetUserMatchesAsync(ClaimsPrincipal user, int? matchID, CancellationToken ct = default)
    {
        return Task.FromResult(new List<Models.MatchModel>());
    }
    public virtual Task<Models.MatchModel?> GetUserMatchAsync(ClaimsPrincipal user, int? matchID, CancellationToken ct = default)
    {
        return Task.FromResult<Models.MatchModel?>(CreateMatch());
    }
    public virtual Task<Models.MatchModel?> GetMatchAsync(int? matchID, CancellationToken ct = default)
    {
        return Task.FromResult<Models.MatchModel?>(CreateMatch());
    }
    public virtual bool IsMatchFinished(Models.MatchModel match)
    {
        if (match.MatchQuestions.Count == 0) return true;

        if (match.MatchQuestionAnswers.Count < 1) return false;

        switch (match.MatchMode)
        {
            case Models.MatchMode.OneAndDone:
                return match.MatchQuestions.GetAttemptedQuestions(match.MatchQuestionAnswers).Count == match.MatchQuestions.Count;
                break;
            default:
                var result = match.MatchQuestions.GetIncorrectQuestions(match.MatchQuestionAnswers);
                if (result.Count == 0) result = match.MatchQuestions.GetIncorrectQuestions(match.MatchQuestionAnswers);
                return result.Count == 0;
        }
    }

    public virtual Task<Models.MatchModel> CreateMatchAsync(Models.MatchModel newMatch, ClaimsPrincipal user, CancellationToken ct = default)
    {
        return Task.FromResult(CreateMatch());
    }

    public virtual Task<List<Models.UserModel>> GetUsersAsync(CancellationToken ct)
    {
        return Task.FromResult<List<Models.UserModel>>([]);
    }

    public virtual Task<Models.MatchModel> UpdateMatchAsync(Models.MatchModel match, CancellationToken ct)
    {
        return Task.FromResult(match);
    }

    public virtual Task<List<Models.MatchModel>> GetMatchesAsync(CancellationToken ct)
    {
        return Task.FromResult(new List<Models.MatchModel>());
    }
    public virtual Task<int> DeleteUserMatchAsync(ClaimsPrincipal user, int? id, CancellationToken ct)
    {
        return Task.FromResult(0);
    }
}
