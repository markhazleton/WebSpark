using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TriviaSpark.Domain.Entities;
using TriviaSpark.Domain.Models;

namespace TriviaSpark.Domain.Services;

public class TriviaMatchService(
    ILogger<TriviaMatchService> logger,
    IQuestionSourceAdapter questionSource,
    TriviaSparkDbContext dbContent)
    : BaseMatchService(new MatchModel()), ITriviaMatchService
{
    private Question Create(QuestionModel question)
    {
        Question results = new();
        try
        {
            List<QuestionAnswer> answers =
            [
                new QuestionAnswer()
                {
                    AnswerText = question.CorrectAnswer,
                    IsCorrect = true,
                    IsValid = true,
                    ErrorMessage = string.Empty,
                }
            ];
            foreach (var answer in question.IncorrectAnswers)
            {
                answers.Add(new QuestionAnswer()
                {
                    AnswerText = answer,
                    IsCorrect = false,
                    IsValid = true,
                    ErrorMessage = string.Empty,
                });
            }
            return new Question()
            {
                QuestionId = question.QuestionId,
                Category = question.Category,
                Type = question.Type,
                Difficulty = question.Difficulty,
                QuestionText = question.QuestionText,
                Answers = answers,
                Source = question.Source,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateQuestionFromTriviaQuestion:Exception");

        }
        return results;
    }
    private MatchModel? Create(Match? dbMatch, QuestionAnswer? dbAnswer = null)
    {
        if (dbMatch is null)
        {
            return default;
        }

        if (dbMatch.MatchId == 0)
        {
            return default;
        }

        MatchModel match = new();
        try
        {
            match.MatchId = dbMatch.MatchId;
            match.MatchName = dbMatch.MatchName;
            match.MatchDate = dbMatch.CreatedDate;
            match.UserId = dbMatch.UserId;
            match.MatchMode = dbMatch.MatchMode;
            match.Difficulty = dbMatch.Difficulty;
            match.QuestionType = dbMatch.QuestionType;
            match.User = new UserModel()
            {
                UserId = dbMatch.User.Id,
                UserName = dbMatch.User.UserName,
                Email = dbMatch.User.Email,
                PhoneNumber = dbMatch.User.PhoneNumber
            };

            match.MatchQuestions.Add(dbMatch.MatchQuestions
                .Where(s => s is not null)
                .Select(s => Create(s))
                .ToList() ?? []);

            match.MatchQuestionAnswers = dbMatch.MatchQuestionAnswers
                .Select(s => new MatchQuestionAnswerModel()
                {
                    MatchId = s.MatchId,
                    QuestionId = s.QuestionId,
                    AnswerId = s.AnswerId,
                }).ToList();


            if (match.MatchQuestions.Count == 0 || IsMatchFinished(match))
            {

            }
            else
            {
                if (dbAnswer?.IsCorrect ?? false)
                {
                    match.CurrentQuestion = GetNextQuestion(match) ?? new QuestionModel() { };
                }
                else
                {
                    match.CurrentQuestion = GetNextQuestion(match) ?? new QuestionModel() { };
                }
                match.CurrentAnswer = new QuestionAnswerModel()
                {
                    QuestionId = match.CurrentQuestion.QuestionId
                };
            }
            match.NumberOfQuestions = match.MatchQuestions.Count;

            match.ScoreCard = match.MatchQuestions.CalculateScore(match.MatchQuestionAnswers);

        }
        catch (Exception ex)
        {
            logger.LogError("CreateMatchModelFromMatch:Exception", ex);
        }
        return match;
    }

    private static QuestionModel Create(MatchQuestion matchQuestion)
    {
        if (matchQuestion is null)
        {
            return new QuestionModel();
        }
        if (matchQuestion.Question is null)
        {
            return new QuestionModel()
            {
                QuestionId = matchQuestion.QuestionId,

            };
        }

        return new QuestionModel()
        {
            QuestionId = matchQuestion.QuestionId,
            Category = matchQuestion.Question.Category,
            Type = matchQuestion.Question.Type,
            Difficulty = matchQuestion.Question.Difficulty,
            QuestionText = matchQuestion.Question.QuestionText,
            Answers = matchQuestion.Question.Answers.Select(s => new QuestionAnswerModel()
            {
                AnswerId = s.AnswerId,
                QuestionId = s.QuestionId,
                AnswerText = s.AnswerText,
                IsCorrect = s.IsCorrect,
                IsValid = s.IsValid,
                ErrorMessage = s.ErrorMessage
            }).ToList(),
            Source = matchQuestion.Question.Source,
        };
    }

    private static new Match CreateMatch()
    {
        return new Match()
        {
            UserId = "66eae7b7-c163-4913-8aaf-421a23f0d5d9",
            MatchQuestions = [],
            MatchQuestionAnswers = [],
            MatchName = "Trivia Match",
            Difficulty = Difficulty.Easy,
            QuestionType = QuestionType.Multiple,
            MatchMode = MatchMode.OneAndDone
        };
    }

    private async Task<Match> GetMatchAsync(int MatchId, CancellationToken ct = default)
    {
        var match = await dbContent.Matches
            .Where(w => w.MatchId == MatchId)
            .Include(i => i.User)
            .Include(i => i.MatchQuestions).ThenInclude(i => i.Question).ThenInclude(i => i.Answers)
            .Include(i => i.MatchQuestionAnswers)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
        return match ?? CreateMatch();
    }

    public override async Task<List<MatchModel>> GetMatchesAsync(CancellationToken ct)
    {
        var match = await dbContent.Matches
            .Include(i => i.User)
            .Include(i => i.MatchQuestionAnswers)
            .Include(i => i.MatchQuestions).ThenInclude(i => i.Question).ThenInclude(i => i.Answers)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
        return match.Select(s => Create(s)).ToList() ?? [];
    }
    public override async Task<int> DeleteUserMatchAsync(ClaimsPrincipal? user, int? id, CancellationToken ct)
    {
        var match = await dbContent.Matches.FindAsync([id], cancellationToken: ct);

        if (match is null)
        {
            return 0;
        }
        var currentUserName = user?.Identity?.Name ?? "Anonymous";
        var currentUserId = await dbContent.AspNetUsers
            .Where(w => w.UserName == currentUserName)
            .Select(s => s.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (match.UserId == currentUserId)
        {
            dbContent.Matches.Remove(match);
            await dbContent.SaveChangesAsync(ct);
            return 1;
        }

        return 0;
    }


    private async Task<QuestionAnswer?> SetMatchAnswer(int matchId, QuestionAnswerModel currentAnswer, CancellationToken ct)
    {
        try
        {
            var dbQuestion = await dbContent.MatchQuestions
                .Where(w => w.MatchId == matchId && w.QuestionId == currentAnswer.QuestionId)
                .Include(i => i.Question).ThenInclude(i => i.Answers)
                .Select(s => s.Question)
                .AsSingleQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(ct) ?? throw new Exception("Question Not Found");

            var dbAnswer = dbQuestion.Answers
                .Where(w => w.AnswerText == currentAnswer.AnswerText)
                .FirstOrDefault() ?? throw new Exception("Answer Not Found");

            var dbMatchAnswer = await dbContent.MatchAnswers
                .Where(w => w.MatchId == matchId)
                .Where(w => w.QuestionId == dbQuestion.QuestionId)
                .Where(w => w.AnswerId == dbAnswer.AnswerId)
                .AsSingleQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (dbMatchAnswer is null)
            {
                if (double.TryParse(currentAnswer.ElapsedTime, out double timeTook))
                {
                    timeTook *= 1000;
                }
                dbContent.MatchAnswers.Add(new MatchQuestionAnswer()
                {
                    AnswerId = dbAnswer.AnswerId,
                    MatchId = matchId,
                    QuestionId = dbQuestion.QuestionId,
                    ElapsedTime = (int)timeTook
                });
                await dbContent.SaveChangesAsync(ct);
            }
            return dbAnswer;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "SetMatchAnswer Exception");
        }
        finally
        {

        }
        return null;
    }

    public override async Task<MatchModel?> AddAnswerAsync(int matchId, QuestionAnswerModel? currentAnswer, CancellationToken ct = default)
    {
        if (currentAnswer is null) return null;

        try
        {
            QuestionAnswer? dbAnswer = await SetMatchAnswer(matchId, currentAnswer, ct);
            Match match = await GetMatchAsync(matchId, ct);
            return Create(match, dbAnswer);
        }
        catch (Exception ex)
        {
            logger.LogError("AddAnswerAsync:Exception", ex);
        }
        finally
        {
            dbContent.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();
        }
        return null;
    }

    public override async Task<List<UserModel>> GetUsersAsync(CancellationToken ct)
    {
        try
        {
            return await dbContent.AspNetUsers.Select(s => new UserModel()
            {
                UserId = s.Id,
                UserName = s.UserName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber
            }).ToListAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError("AddAnswerAsync:Exception", ex);
        }
        finally
        {
            dbContent.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();
        }
        return [];
    }

    public override async Task<MatchModel?> GetMoreQuestionsAsync(int MatchId, int NumberOfQuestionsToAdd = 1, Difficulty difficulty = Difficulty.Easy, CancellationToken ct = default)
    {
        try
        {
            QuestionProvider source = new();
            var newQuestions = await questionSource.GetQuestions(NumberOfQuestionsToAdd, difficulty, ct);
            foreach (var question in newQuestions)
            {
                var existingQuestion = await dbContent.Questions.FindAsync([question.QuestionId], cancellationToken: ct);
                if (existingQuestion is null)
                {
                    Question dbQuestion = Create(question);
                    dbContent.Questions.Add(dbQuestion);
                }

                var matchQuestion = dbContent.MatchQuestions.Where(w => w.MatchId == MatchId && w.QuestionId == question.QuestionId)
                    .AsNoTracking()
                    .FirstOrDefault();
                if (matchQuestion is null)
                {
                    matchQuestion = new MatchQuestion()
                    {
                        MatchId = MatchId,
                        QuestionId = question.QuestionId,
                    };
                    dbContent.MatchQuestions.Add(matchQuestion);
                }
            }
            await dbContent.SaveChangesAsync(ct);
            return Create(await GetMatchAsync(MatchId, ct));
        }
        catch (Exception ex)
        {
            logger.LogCritical("GetMoreQuestionsAsync:Exception", ex);
        }
        finally
        {
            dbContent.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();
        }
        return null;
    }
    public override async Task<MatchModel> UpdateMatchAsync(MatchModel match, CancellationToken ct)
    {
        try
        {
            var dbMatch = await dbContent.Matches.FindAsync([match.MatchId], cancellationToken: ct)
                ?? throw new Exception("Match Not Found");

            dbMatch.MatchName = match?.MatchName ?? dbMatch.MatchName;
            dbMatch.MatchMode = match?.MatchMode ?? dbMatch.MatchMode;
            dbMatch.UserId = match?.UserId ?? dbMatch.UserId;
            dbMatch.Difficulty = match?.Difficulty ?? dbMatch.Difficulty;
            dbMatch.QuestionType = match?.QuestionType ?? dbMatch.QuestionType;

            await dbContent.SaveChangesAsync(ct);
            return Create(await GetMatchAsync(match.MatchId, ct));

        }
        catch (Exception ex)
        {
            logger.LogCritical("UpdateMatchAsync:Exception", ex);
        }
        finally
        {
            dbContent.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();
        }
        return match;


    }


    public QuestionModel? GetNextQuestion(MatchModel match)
    {
        try
        {

            var result = match.MatchQuestions.GetIncorrectQuestions(match.MatchQuestionAnswers);
            var unansweredQuestion = match.MatchQuestions.GetUnansweredQuestions(match.MatchQuestionAnswers);
            var random = new Random();

            switch (match.MatchMode)
            {
                case MatchMode.OneAndDone:
                    if (unansweredQuestion.Count > 0)
                    {
                        var index = random.Next(unansweredQuestion.Count);
                        return unansweredQuestion.ElementAt(index);
                    }
                    break;

                case MatchMode.Sequential:
                case MatchMode.Adaptive:
                default:
                    if (unansweredQuestion.Count > 0)
                    {
                        var index = random.Next(unansweredQuestion.Count);
                        return unansweredQuestion.ElementAt(index);
                    }
                    else if (result.Count > 0)
                    {
                        var index = random.Next(result.Count);
                        return result.ElementAt(index);
                    }
                    break;
            }


        }
        catch (Exception ex)
        {
            logger.LogCritical("GetNextQuestion:Exception", ex);
        }
        finally
        {
            dbContent.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();
        }

        return null;
    }
    public override async Task<List<MatchModel>> GetUserMatchesAsync(ClaimsPrincipal user, int? MatchId = null, CancellationToken ct = default)
    {
        List<MatchModel> userMatchList = [];
        try
        {
            var currentUserName = user?.Identity?.Name ?? string.Empty;
            var currentUserId = await dbContent.AspNetUsers
                .Where(w => w.UserName == currentUserName)
                .Select(s => s.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken: ct);


            // .ThenInclude(i => i.Question).ThenInclude(i => i.Answers)

            var userMatches = await dbContent.Matches
                .Where(w => w.UserId == currentUserId)
                .Include(i => i.User)
                .Include(i => i.MatchQuestions)
                .Include(i => i.MatchQuestionAnswers)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync(ct);

            foreach (Match match in userMatches)
            {
                if (match is null) continue;

                userMatchList.Add(Create(match) ?? new());
            }

        }
        catch (Exception ex)
        {
            logger.LogCritical("GetUserMatchesAsync:Exception", ex);
        }
        finally
        {
            dbContent.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();
        }
        return userMatchList;

    }

    public override async Task<MatchModel> CreateMatchAsync(MatchModel newMatch, ClaimsPrincipal? user, CancellationToken ct = default)
    {
        var currentUserName = user?.Identity?.Name ?? "Anonymous";
        var currentUserId = await dbContent.AspNetUsers
            .Where(w => w.UserName == currentUserName)
            .Select(s => s.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken: ct);
        Match match = Create(newMatch, currentUserId);
        dbContent.Matches.Add(match);
        await dbContent.SaveChangesAsync(ct);

        newMatch = await GetMoreQuestionsAsync(match.MatchId, newMatch.NumberOfQuestions, newMatch.Difficulty, ct);

        return newMatch;
    }

    private Match Create(MatchModel newMatch, string? currentUserId)
    {
        return new Match()
        {
            MatchQuestions = [],
            MatchQuestionAnswers = [],
            MatchName = "UserMatch",
            UserId = currentUserId ?? string.Empty,
            MatchMode = newMatch.MatchMode,
            Difficulty = newMatch.Difficulty,
            QuestionType = newMatch.QuestionType,
            CreatedDate = DateTime.Now,
            ModifiedDate = DateTime.Now,
        };
    }

    public override async Task<MatchModel?> GetUserMatchAsync(ClaimsPrincipal user, int? MatchId = null, CancellationToken ct = default)
    {
        try
        {
            var currentUserName = user?.Identity?.Name ?? string.Empty;
            var currentUserId = await dbContent.AspNetUsers
                .Where(w => w.UserName == currentUserName)
                .Select(s => s.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken: ct);

            Match? match;
            if ((MatchId is null || MatchId == 0) && currentUserId is not null)
            {
                match = await dbContent.Matches
                    .Where(w => w.UserId == currentUserId)
                    .Include(i => i.User)
                    .Include(i => i.MatchQuestions).ThenInclude(i => i.Question).ThenInclude(i => i.Answers)
                    .Include(i => i.MatchQuestionAnswers)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(ct);
            }
            else
            {
                match = await dbContent.Matches
                    .Where(w => w.MatchId == MatchId)
                    .Include(i => i.User)
                    .Include(i => i.MatchQuestions).ThenInclude(i => i.Question).ThenInclude(i => i.Answers)
                    .Include(i => i.MatchQuestionAnswers)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(ct);
            }
            if (match is not null) return Create(match);
            MatchModel newMatch = new();
            match = Create(newMatch, currentUserId);
            dbContent.Matches.Add(match);
            await dbContent.SaveChangesAsync(ct);
            MatchId = match.MatchId;

            match = await dbContent.Matches
                .Where(w => w.MatchId == MatchId)
                .Include(i => i.User)
                .Include(i => i.MatchQuestions).ThenInclude(i => i.Question).ThenInclude(i => i.Answers)
                .Include(i => i.MatchQuestionAnswers)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(cancellationToken: ct);

            return Create(match);
        }
        catch (Exception ex)
        {
            logger.LogCritical("GetUserMatchAsync:Exception", ex);
        }
        finally
        {
            dbContent.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();
        }
        return null;
    }

    public override async Task<MatchModel?> GetMatchAsync(int? MatchId = null, CancellationToken ct = default)
    {
        try
        {
            Match? match;
            if (MatchId is null || MatchId == 0) throw new ArgumentNullException(nameof(MatchId));

            match = await dbContent.Matches
                .Where(w => w.MatchId == MatchId)
                .Include(i => i.User)
                .Include(i => i.MatchQuestions).ThenInclude(i => i.Question).ThenInclude(i => i.Answers)
                .Include(i => i.MatchQuestionAnswers)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(ct);

            if (match is not null) return Create(match);

            throw new Exception("Match Not Found");
        }
        catch (Exception ex)
        {
            logger.LogCritical("GetUserMatchAsync:Exception", ex);
        }
        finally
        {
            dbContent.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();
        }
        return null;
    }

}
