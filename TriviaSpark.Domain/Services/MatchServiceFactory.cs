namespace TriviaSpark.Domain.Services;

public class MatchServiceFactory(
    Dictionary<Models.MatchMode,
        Func<IMatchService>> serviceFactories) : IMatchServiceFactory
{
    public IMatchService CreateMatchService(Models.MatchMode mode)
    {
        if (serviceFactories.TryGetValue(mode, out Func<IMatchService> serviceFactory))
        {
            return serviceFactory();
        }
        throw new ArgumentException($"Invalid match mode: {mode}", nameof(mode));
    }
}
