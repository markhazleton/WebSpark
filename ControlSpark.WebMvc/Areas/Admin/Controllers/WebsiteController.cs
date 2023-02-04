
namespace ControlSpark.WebMvc.Areas.Admin.Controllers;

[Area("Admin")]
public class WebsiteController : Controller
{
    private readonly ILogger<WebsiteController> _logger;
    private readonly IScopeInformation _scopeInfo;
    private readonly IWebsiteService _websiteService;

    public WebsiteController(IWebsiteService service, ILogger<WebsiteController> logger, IScopeInformation scopeInfo)
    {
        _websiteService = service;
        _logger = logger;
        _scopeInfo = scopeInfo;
    }


    // GET: WebsiteController
    public ActionResult Index()
    {
        return View(_websiteService.Get());
    }

    // GET: WebsiteController/Details/5
    public ActionResult Details(int id)
    {
        return View();
    }

    // GET: WebsiteController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: WebsiteController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: WebsiteController/Edit/5
    public ActionResult Edit(int id)
    {
        return View();
    }

    // POST: WebsiteController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: WebsiteController/Delete/5
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: WebsiteController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}
