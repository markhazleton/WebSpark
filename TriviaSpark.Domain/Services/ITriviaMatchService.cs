using System.Security.Claims;
using TriviaSpark.Domain.Models;

namespace TriviaSpark.Domain.Services;

public interface ITriviaMatchService
{
    Task<MatchModel?> GetUserMatchAsync(ClaimsPrincipal user, int? matchID, CancellationToken ct = default);
    Task<MatchModel?> GetMatchAsync(int? matchID, CancellationToken ct = default);
    Task<MatchModel?> GetMoreQuestionsAsync(int MatchId, int NumberOfQuestionsToAdd = 1, Difficulty difficulty = Difficulty.Easy, CancellationToken ct = default);
    Task<MatchModel?> AddAnswerAsync(int MatchId, QuestionAnswerModel currentAnswer, CancellationToken ct = default);
    QuestionModel? GetNextQuestion(MatchModel match);
    bool IsMatchFinished(MatchModel match);
    string GetMatchStatus(MatchModel match);
    Task<List<MatchModel>> GetUserMatchesAsync(ClaimsPrincipal user, int? MatchId = null, CancellationToken ct = default);
    Task<MatchModel> CreateMatchAsync(MatchModel newMatch, ClaimsPrincipal? user, CancellationToken ct = default);
    Task<List<Models.UserModel>> GetUsersAsync(CancellationToken ct);
    Task<MatchModel> UpdateMatchAsync(MatchModel match, CancellationToken ct);
    Task<List<MatchModel>> GetMatchesAsync(CancellationToken ct);
    Task<int> DeleteUserMatchAsync(ClaimsPrincipal? user, int? id, CancellationToken ct);
}
