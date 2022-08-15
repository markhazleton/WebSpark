namespace ControlSpark.WebMvc.Controllers.Api.Cms;

/// <summary>
/// Domain API Controller
/// </summary>
[Route("api/[controller]")]
public class DomainController : ApiBaseController
{
    private readonly ILogger<DomainController> _logger;
    private readonly IScopeInformation _scopeInfo;
    private readonly IWebsiteService _dbDomain;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="scopeInfo"></param>
    /// <param name="dbDomain"></param>
    public DomainController(ILogger<DomainController> logger, IScopeInformation scopeInfo, IWebsiteService dbDomain)
    {
        _dbDomain = dbDomain;
        _logger = logger;
        _scopeInfo = scopeInfo;
    }

    /// <summary>
    /// Get All Domains
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<WebsiteModel> Get()
    {
        var myList = _dbDomain.Get();
        foreach (var d in myList)
        {
            d.Url = GetObjectUrl(d.Id);
        }
        return myList;
    }

    /// <summary>
    /// Get Domain by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<WebsiteModel> Get(int id)
    {
        var d = await _dbDomain.GetAsync(id);
        d.Url = GetObjectUrl(d.Id);
        foreach (var m in d.Menu)
        {
            m.ApiUrl = GetObjectUrl(m.Id, "Menu");
        }
        return d;
    }


    /// <summary>
    /// Create New Domain 
    /// </summary>
    /// <param name="value"></param>
    [HttpPost]
    public void Post([FromBody] WebsiteModel value)
    {
    }

    /// <summary>
    /// Update Existing Domain
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] WebsiteModel value)
    {
    }

    /// <summary>
    /// Delete Existing Domain
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
