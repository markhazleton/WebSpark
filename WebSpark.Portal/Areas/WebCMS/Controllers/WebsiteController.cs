using WebSpark.Core.Models.EditModels;

namespace WebSpark.Portal.Areas.WebCMS.Controllers;

/// <summary>
/// Website Controller
/// </summary>
/// <remarks>
/// Website Controller Constructor
/// </remarks>
/// <param name="service"></param>
/// <param name="logger"></param>
/// <param name="scopeInfo"></param>
public class WebsiteController(
    Core.Interfaces.IWebsiteService service,
    ILogger<WebsiteController> logger,
    Core.Interfaces.IScopeInformation scopeInfo) : WebCMSBaseController
{


    /// <summary>
    /// Index Action
    /// </summary>
    /// <returns></returns>
    public ActionResult Index() { return View(service.Get()); }


    /// <summary>
    /// Details Action
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ActionResult> Details(int id)
    {
        var website = await service.GetEditAsync(id);
        return View(website);
    }

    /// <summary>
    /// Create Action
    /// </summary>
    /// <returns></returns>
    public ActionResult Create() { return View(new WebsiteEditModel()); }

    /// <summary>
    /// Create Post Action to create a new menu item
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(WebsiteEditModel collection)
    {
        if (collection == null)
        {
            return RedirectToAction(nameof(Index));
        }
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    /// <summary>
    /// Edit Action
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ActionResult> Edit(int id)
    {
        var website = await service.GetEditAsync(id);
        return View(website);
    }

    /// <summary>
    /// edit post action to update a menu item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="website"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(int id, WebsiteEditModel website)
    {
        try
        {
            var itemToUpdate = await service.GetAsync(id);

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
                itemToUpdate.IsRecipeSite = website.IsRecipeSite;
                itemToUpdate.ModifiedID = 99;
                var saveResult = service.Save(itemToUpdate);
            }
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    /// <summary>
    /// delete action
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult Delete(int id) { return View(); }

    /// <summary>
    ///  delete post action to delete a menu item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="collection"></param>
    /// <returns></returns>
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
