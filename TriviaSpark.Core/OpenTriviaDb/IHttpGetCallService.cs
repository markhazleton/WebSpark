namespace TriviaSpark.Core.OpenTriviaDb;

public interface IHttpGetCallService
{
    Task<HttpGetCallResults<T>> GetAsync<T>(HttpGetCallResults<T> statusCall, CancellationToken ct);
}
