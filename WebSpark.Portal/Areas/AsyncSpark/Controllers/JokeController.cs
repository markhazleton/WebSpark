using System.Net;
using System.Text.Json.Serialization;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;


/// <summary>
/// Controller for handling joke API calls.
/// </summary>
public class JokeController(
    ILogger<JokeController> logger,
    IHttpRequestResultService getCallService) : AsyncSparkBaseController
{
    /// <summary>
    /// Retrieves a random joke from the Joke API.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A view with the joke results.</returns>
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var jokeRequestResult = new HttpRequestResult<JokeModel>
        {
            CacheDurationMinutes = 0,
            RequestPath = "https://v2.jokeapi.dev/joke/Any?safe-mode"
        };
        try
        {
            jokeRequestResult = await getCallService.HttpSendRequestResultAsync(jokeRequestResult).ConfigureAwait(false);
            return View(jokeRequestResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching the joke.");
            jokeRequestResult.ResponseResults = new JokeModel { Error = true };
            jokeRequestResult.StatusCode = HttpStatusCode.InternalServerError;
            jokeRequestResult.ErrorList.Add(ex.Message);
            return View(jokeRequestResult);
        }
    }
}

public class JokeModel
{
    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("setup")]
    public string? Setup { get; set; }

    [JsonPropertyName("delivery")]
    public string? Delivery { get; set; }

    [JsonPropertyName("joke")]
    public string? Joke { get; set; }

    [JsonPropertyName("flags")]
    public FlagsModel? Flags { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("safe")]
    public bool Safe { get; set; }

    [JsonPropertyName("lang")]
    public string? Lang { get; set; }
}

public class FlagsModel
{
    [JsonPropertyName("nsfw")]
    public bool Nsfw { get; set; }

    [JsonPropertyName("religious")]
    public bool Religious { get; set; }

    [JsonPropertyName("political")]
    public bool Political { get; set; }

    [JsonPropertyName("racist")]
    public bool Racist { get; set; }

    [JsonPropertyName("sexist")]
    public bool Sexist { get; set; }

    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }
}

