using System.Security.Claims;

namespace TriviaSpark.Core.Services;

public interface IMatchService
{
    Task<Models.MatchModel?> GetUserMatchAsync(ClaimsPrincipal user, int? matchID, CancellationToken ct = default);
    Task<Models.MatchModel?> GetMatchAsync(int? matchID, CancellationToken ct = default);
    Task<Models.MatchModel?> GetMoreQuestionsAsync(int MatchId, int NumberOfQuestionsToAdd = 1, Models.Difficulty difficulty = Models.Difficulty.Easy, CancellationToken ct = default);
    Task<Models.MatchModel?> AddAnswerAsync(int MatchId, Models.QuestionAnswerModel currentAnswer, CancellationToken ct = default);
    Models.QuestionModel? GetNextQuestion(Models.MatchModel match);
    bool IsMatchFinished(Models.MatchModel match);
    string GetMatchStatus(Models.MatchModel match);
    Task<List<Models.MatchModel>> GetUserMatchesAsync(ClaimsPrincipal user, int? MatchId = null, CancellationToken ct = default);
    Task<Models.MatchModel> CreateMatchAsync(Models.MatchModel newMatch, ClaimsPrincipal user, CancellationToken ct = default);
    Task<List<Models.UserModel>> GetUsersAsync(CancellationToken ct);
    Task<Models.MatchModel> UpdateMatchAsync(Models.MatchModel match, CancellationToken ct);
    Task<List<Models.MatchModel>> GetMatchesAsync(CancellationToken ct);
    Task<int> DeleteUserMatchAsync(ClaimsPrincipal user, int? id, CancellationToken ct);
}
