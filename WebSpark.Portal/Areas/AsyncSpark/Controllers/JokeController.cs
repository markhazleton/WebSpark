using HttpClientUtility.SendService;
using System.Net;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;


/// <summary>
/// Controller for handling joke API calls.
/// </summary>
public class JokeController(
    ILogger<JokeController> logger, 
    IHttpClientSendService getCallService) : AsyncSparkBaseController
{
    /// <summary>
    /// Retrieves a random joke from the Joke API.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A view with the joke results.</returns>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var jokeResult = new HttpClientSendRequest<JokeModel>
        {
            CacheDurationMinutes = 0,
            RequestPath = "https://v2.jokeapi.dev/joke/Any?safe-mode"
        };
        try
        {
            jokeResult = await getCallService.HttpClientSendAsync<JokeModel>(jokeResult, ct).ConfigureAwait(false);
            return View(jokeResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching the joke.");
            jokeResult.ResponseResults = new JokeModel { error = true };
            jokeResult.StatusCode = HttpStatusCode.InternalServerError;
            jokeResult.ErrorList.Add(ex.Message);
            return View(jokeResult);
        }
    }
}

public class JokeModel
{
    public bool error { get; set; }
    public string? category { get; set; }
    public string? type { get; set; }
    public string? setup { get; set; }
    public string? delivery { get; set; }
    public string? joke { get; set; }
    public FlagsModel? flags { get; set; }
    public int id { get; set; }
    public bool safe { get; set; }
    public string? lang { get; set; }
}
public class FlagsModel
{
    public bool nsfw { get; set; }
    public bool religious { get; set; }
    public bool political { get; set; }
    public bool racist { get; set; }
    public bool sexist { get; set; }
    public bool @explicit { get; set; }
}
