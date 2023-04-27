
using ControlSpark.Domain.EditModels;

namespace ControlSpark.WebMvc.Areas.Admin.Controllers;
public class WebsiteController : BaseAdminController
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
    public async Task<ActionResult> Details(int id)
    {
        var website = await _websiteService.GetEditAsync(id);
        return View(website);
    }

    // GET: WebsiteController/Create
    public ActionResult Create()
    {
        return View(new WebsiteEditModel());
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
    public async Task<ActionResult> Edit(int id)
    {
        var website = await _websiteService.GetEditAsync(id);
        return View(website);
    }

    // POST: WebsiteController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(int id, WebsiteEditModel website)
    {
        try
        {
            var itemToUpdate = await _websiteService.GetAsync(id);

            if (itemToUpdate != null)
            {
                itemToUpdate.Name = website.Name ?? itemToUpdate.Name;
                itemToUpdate.Description = website.Description ?? itemToUpdate.Description;
                itemToUpdate.SiteTemplate = website.SiteTemplate ?? itemToUpdate.SiteTemplate;
                itemToUpdate.SiteStyle = website.SiteStyle ?? itemToUpdate.SiteStyle;
                itemToUpdate.Message = website.Message ?? website.Message;
                itemToUpdate.SiteName = website.SiteName ?? itemToUpdate.SiteName;
                itemToUpdate.WebsiteUrl = website.WebsiteUrl ?? itemToUpdate.WebsiteUrl;
                itemToUpdate.WebsiteTitle = website.WebsiteTitle ?? itemToUpdate.WebsiteTitle;
                itemToUpdate.UseBreadCrumbURL = website.UseBreadCrumbURL;
                itemToUpdate.ModifiedID = 99;
                var saveResult = _websiteService.Save(itemToUpdate);
            }
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
