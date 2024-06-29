namespace TriviaSpark.Core.Services
{
    public interface IMatchServiceFactory
    {
        IMatchService CreateMatchService(Models.MatchMode mode);
    }
}
