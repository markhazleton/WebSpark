using HttpClientUtility.SendService;
using WebSpark.Portal.Areas.AsyncSpark.Models.Nasa;

namespace WebSpark.Portal.Areas.AsyncSpark.Controllers;

public class NasaPictureController(
    ILogger<NasaPictureController> logger, 
    IHttpClientSendService service) : AsyncSparkBaseController
{
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var apiRequest = new HttpClientSendRequest<NasaPictureListDto>
        {
            CacheDurationMinutes = 500,
            RequestPath = "https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY&count=5"
        };

        if (service == null)
        {
            logger.LogError("_service is null");
            throw new NullReferenceException(nameof(service));
        }

        apiRequest = await service.HttpClientSendAsync(apiRequest, ct).ConfigureAwait(false);
        return View(apiRequest);
    }
}
