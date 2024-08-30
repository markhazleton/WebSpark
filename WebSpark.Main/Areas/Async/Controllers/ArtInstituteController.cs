using HttpClientUtility.SendService;
using WebSpark.Main.Areas.Async.Models.Art;

namespace WebSpark.Main.Areas.Async.Controllers
{
    public class ArtInstituteController : AsyncBaseController
    {
        private readonly ILogger<ArtInstituteController> _logger;
        private readonly IHttpClientSendService _getCallService;
        private const string BaseUrl = "https://api.artic.edu/api/v1/artworks";

        public ArtInstituteController(ILogger<ArtInstituteController> logger, IHttpClientSendService getCallService)
        {
            _logger = logger;
            _getCallService = getCallService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("SearchByStyle");
        }
        

        // 1. Search Artworks by Style
        [HttpPost]
        public async Task<IActionResult> SearchByStyle(string style = "Impressionism", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                _logger.LogWarning("Style parameter is missing or empty.");
                return BadRequest("Style parameter is required.");
            }
            var searchUrl = $"https://api.artic.edu/api/v1/artworks/search?query[term][is_public_domain]=true&[term][material_titles]=oil paint&limit=20&fields=id,title,image_id,artist_title,material_titles&q={style}";

//            var searchUrl = $"{BaseUrl}/search?query[term][style_title]={style}&limit=20&fields=id,title,image_id,artist_title,material_titles,style_title&q={style}";
            var artListRequest = new HttpClientSendRequest<ArtWorksResponse> { RequestPath = searchUrl };
            artListRequest.CacheDurationMinutes = 60;
            artListRequest.Retries = 1;
            var artResult = await _getCallService.HttpClientSendAsync(artListRequest, ct).ConfigureAwait(false);
            var artResponse = artResult.ResponseResults;
            if (artResponse?.data is null)
            {
                _logger.LogError("No artworks found for the specified style.");
                return NotFound("No artworks found for the specified style.");
            }

            var artList = artResponse.data.Select(item => new ArtWork
            {
                id = item.id.ToString(),
                title = item.title,
                image_id = item.image_id,
                artist_title = item.artist_title,
                material_titles = item.material_titles,
                style_title = item.style_title
            }).ToList();

            return View("ListArtworks", artList); // View showing a list of artworks
        }


        // 2. List Artworks
        [HttpGet]
        public async Task<IActionResult> ListArtworks(CancellationToken ct = default)
        {
            var listUrl = $"{BaseUrl}?limit=20&fields=id,title,image_id,artist_title,material_titles";
            var artListRequest = new HttpClientSendRequest<ArtWorksResponse> { RequestPath = listUrl };
            artListRequest.CacheDurationMinutes = 60;
            artListRequest.Retries = 1;

            var artResult = await _getCallService.HttpClientSendAsync(artListRequest, ct).ConfigureAwait(false);
            var artResponse = artResult.ResponseResults;
            if (artResponse?.data is null)
            {
                _logger.LogError("Failed to retrieve artworks.");
                return NotFound("Failed to retrieve artworks.");
            }

            var artList = artResponse.data.Select(item => new ArtWork
            {
                id = item.id.ToString(),
                title = item.title,
                image_id = item.image_id,
                artist_title = item.artist_title,
                material_titles = item.material_titles
            }).ToList();

            return View(artList); // View showing the list of artworks
        }

        // 3. Get Art Details
        [HttpGet]
        public async Task<IActionResult> ArtDetails(int id, CancellationToken ct = default)
        {
            var detailsUrl = $"{BaseUrl}/{id}?fields=id,title,image_id,artist_title,material_titles,style_title,artist_display,date_display,dimensions,medium_display";
            var artDetailsRequest = new HttpClientSendRequest<ArtDetailsResponse> { RequestPath = detailsUrl };
            artDetailsRequest.CacheDurationMinutes = 60;
            artDetailsRequest.Retries = 1;
            var artResult = await _getCallService.HttpClientSendAsync(artDetailsRequest, ct).ConfigureAwait(false);
            var artResponse = artResult.ResponseResults;
            if (artResponse is null)
            {
                _logger.LogError($"Artwork with ID {id} not found.");
                return NotFound($"Artwork with ID {id} not found.");
            }

            var artDetail = new ArtWork
            {
                id = artResponse.data.id.ToString(),
                title = artResponse.data.title,
                image_id = artResponse.data.image_id,
                artist_title = artResponse.data.artist_title,
                material_titles = artResponse.data.material_titles,
                style_title = artResponse.data.style_title,
                artist_display = artResponse.data.artist_display,
                date_display = artResponse.data.date_display,
                dimensions = artResponse.data.dimensions,
                medium_display = artResponse.data.medium_display
            };

            return View(artDetail); // View showing details of a single artwork
        }
    }
}
