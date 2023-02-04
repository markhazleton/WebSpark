using ControlSpark.Domain.EditModels;

namespace ControlSpark.WebMvc.Areas.Admin.Controllers;

[Area("Admin")]
public class MenuController : Controller
{
    private readonly ILogger<MenuController> _logger;
    private readonly IScopeInformation _scopeInfo;
    private readonly IMenuService _menuService;

    public MenuController(ILogger<MenuController> logger, IScopeInformation scopeInfo, IMenuService menuService)
    {
        _menuService = menuService;
        _logger = logger;
        _scopeInfo = scopeInfo;
    }

    // GET: MenuController
    public ActionResult Index()
    {
        return View(_menuService.GetAllMenuItems());
    }

    // GET: MenuController/Details/5
    public async Task<ActionResult> Details(int id)
    {
        return View(await _menuService.GetMenuEditAsync(id));
    }

    // GET: MenuController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: MenuController/Create
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

    // GET: MenuController/Edit/5
    public async Task<ActionResult> Edit(int id)
    {
        var item = await _menuService.GetMenuEditAsync(id);
        return View(item);
    }

    // POST: MenuController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, MenuEditModel item)
    {
        try
        {
            var menuToUpdate = _menuService.GetAllMenuItems().Where(w => w.Id == id).FirstOrDefault();
            if (menuToUpdate != null)
            {
                menuToUpdate.Title = item.Title ?? menuToUpdate.Title;
                menuToUpdate.PageContent = item.PageContent ?? menuToUpdate.PageContent;
                menuToUpdate.Action = item.Action ?? menuToUpdate.Action;
                menuToUpdate.ApiUrl = item.ApiUrl ?? menuToUpdate.ApiUrl;
                menuToUpdate.DomainUrl = item.DomainUrl ?? menuToUpdate.DomainUrl;
                menuToUpdate.Controller = item.Controller ?? menuToUpdate.Controller;
                menuToUpdate.Description = item.Description ?? menuToUpdate.Description;
                menuToUpdate.Argument = item.Argument ?? menuToUpdate.Argument;
                menuToUpdate.DisplayInNavigation = item.DisplayInNavigation;
                menuToUpdate.DisplayOrder = item.DisplayOrder;
                var saveResult = _menuService.Save(menuToUpdate);
            }
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }

    // GET: MenuController/Delete/5
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: MenuController/Delete/5
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
