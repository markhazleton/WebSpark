namespace TriviaSpark.Domain.Services
{
    public interface IMatchServiceFactory
    {
        IMatchService CreateMatchService(Models.MatchMode mode);
    }
}
