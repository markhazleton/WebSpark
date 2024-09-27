namespace TriviaSpark.Domain.Services
{
    public interface IMatchServiceFactory
    {
        ITriviaMatchService CreateMatchService(Models.MatchMode mode);
    }
}
