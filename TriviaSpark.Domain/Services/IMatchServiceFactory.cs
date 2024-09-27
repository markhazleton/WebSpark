namespace TriviaSpark.Domain.Services;

public interface ITriviaMatchServiceFactory
{
    ITriviaMatchService CreateMatchService(Models.MatchMode mode);
}
