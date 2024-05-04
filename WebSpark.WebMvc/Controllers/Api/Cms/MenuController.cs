using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;

namespace WebSpark.WebMvc.Controllers.Api.Cms;

/// <summary>
/// Menu Controller
/// </summary>
/// <remarks>
/// Menu controller constructor
/// </remarks>
/// <param name="logger"></param>
/// <param name="scopeInfo"></param>
/// <param name="menuService"></param>
public class MenuController(ILogger<MenuController> logger, IScopeInformation scopeInfo, IMenuProvider menuService) : ApiBaseController
{
    private readonly ILogger<MenuController> _logger = logger;
    private readonly IScopeInformation _scopeInfo = scopeInfo;
    private readonly IMenuProvider _menuService = menuService;

    /// <summary>
    /// Get List of Menu Items
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<MenuModel> Get()
    {
        var myList = _menuService.GetMenuList();
        foreach (var m in myList)
        {
            m.ApiUrl = GetObjectUrl(m.Id);
            m.DomainUrl = GetObjectUrl(m.DomainID, "Domain");
        }
        return _menuService.GetMenuList();
    }


    /// <summary>
    /// Get Menu By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public MenuModel Get(int id)
    {
        var m = _menuService.GetMenuItem(id);
        m.ApiUrl = GetObjectUrl(m.Id);
        m.DomainUrl = GetObjectUrl(m.DomainID, "Domain");
        return m;
    }

    /// <summary>
    /// Add New Menu
    /// </summary>
    /// <param name="value"></param>
    [HttpPost]
    public void Post([FromBody] MenuModel value)
    {
    }

    /// <summary>
    /// Update Existing Menu
    /// </summary>
    /// <param name="id"></param>
    /// <param name="value"></param>
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] MenuModel value)
    {
    }

    /// <summary>
    /// Delete Existing Menu
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
