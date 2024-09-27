namespace TriviaSpark.Domain.Services;

public class MatchServiceFactory(
    Dictionary<Models.MatchMode,
        Func<ITriviaMatchService>> serviceFactories) : ITriviaMatchServiceFactory
{
    public ITriviaMatchService CreateMatchService(Models.MatchMode mode)
    {
        if (serviceFactories.TryGetValue(mode, out Func<ITriviaMatchService> serviceFactory))
        {
            return serviceFactory();
        }
        throw new ArgumentException($"Invalid match mode: {mode}", nameof(mode));
    }
}
