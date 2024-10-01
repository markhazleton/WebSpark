using HttpClientUtility.RequestResult;
using WebSpark.Portal.Areas.AsyncSpark.Controllers;
using WebSpark.Portal.Areas.AsyncSpark.Models.Art;

public class ArtInstituteController(
    ILogger<ArtInstituteController> logger,
    IHttpRequestResultService getCallService) : AsyncSparkBaseController
{
    private readonly ILogger<ArtInstituteController> _logger = logger;
    private readonly IHttpRequestResultService _getCallService = getCallService;
    private const string BaseUrl = "https://api.artic.edu/api/v1/artworks";

    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Index(string style = "Impressionism", CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(style))
        {
            _logger.LogWarning("Style parameter is missing or empty.");
            return BadRequest("Style parameter is required.");
        }

        var searchUrl = $"{BaseUrl}/search?query[term][is_public_domain]=true&[term][material_titles]=oil paint&limit=20&fields=id,title,image_id,artist_title,material_titles&q={style}";
        var artListRequest = new HttpRequestResult<ArtWorksResponse>
        {
            RequestPath = searchUrl,
            CacheDurationMinutes = 90,
            Retries = 1
        };
        artListRequest = await _getCallService.HttpSendRequestResultAsync(artListRequest, ct).ConfigureAwait(false);

        ViewBag.Title = $"{style} Artworks List";
        ViewBag.MetaDescription = $"Explore the extensive collection of {style} artworks featuring various artists and materials. Browse and discover unique art pieces that inspire.";
        ViewBag.MetaKeywords = "artworks, art gallery, artist styles, art materials, paintings, sculptures, digital art, art collection";

        return View(artListRequest);
    }

    [HttpGet]
    public async Task<IActionResult> ArtDetails(int id, CancellationToken ct = default)
    {
        var detailsUrl = $"{BaseUrl}/{id}?fields=id,title,image_id,artist_title,material_titles,style_title,artist_display,date_display,dimensions,medium_display";
        var artDetailsRequest = new HttpRequestResult<ArtDetailsResponse> { RequestPath = detailsUrl, CacheDurationMinutes = 60, Retries = 1 };
        artDetailsRequest = await _getCallService.HttpSendRequestResultAsync(artDetailsRequest, ct).ConfigureAwait(false);
        return View(artDetailsRequest);
    }
}
