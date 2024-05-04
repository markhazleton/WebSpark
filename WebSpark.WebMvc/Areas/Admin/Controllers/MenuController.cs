using WebSpark.Domain.EditModels;
using WebSpark.Domain.Extensions;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;

namespace WebSpark.WebMvc.Areas.Admin.Controllers;

public class MenuController(ILogger<MenuController> logger, IScopeInformation scopeInfo, IMenuService menuService) : BaseAdminController
{
    private readonly ILogger<MenuController> _logger = logger;
    private readonly IScopeInformation _scopeInfo = scopeInfo;
    private readonly IMenuService _menuService = menuService;

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
    public async Task<ActionResult> Create()
    {
        var item = await _menuService.GetMenuEditAsync(0);
        return View(item);
    }

    // POST: MenuController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(MenuEditModel item)
    {
        var menuToUpdate = new MenuModel();
        try
        {
            if (menuToUpdate != null)
            {
                menuToUpdate.DomainID = item.DomainID;
                menuToUpdate.Title = item.Title ?? menuToUpdate.Title;
                menuToUpdate.Icon = item.Icon ?? menuToUpdate.Icon;
                menuToUpdate.PageContent = item.PageContent ?? menuToUpdate.PageContent;
                menuToUpdate.Action = item.Action ?? menuToUpdate.Action;
                menuToUpdate.ApiUrl = item.ApiUrl ?? menuToUpdate.ApiUrl;
                menuToUpdate.Url = item.Url ?? item.Title ?? "UNKNOWN";
                menuToUpdate.Argument = item.Argument ?? menuToUpdate.Argument;
                menuToUpdate.Controller = item.Controller ?? menuToUpdate.Controller;
                menuToUpdate.Controller = item.Controller ?? menuToUpdate.Controller;
                menuToUpdate.Description = item.Description ?? menuToUpdate.Description;
                menuToUpdate.DisplayInNavigation = item.DisplayInNavigation;
                menuToUpdate.DisplayOrder = item.DisplayOrder;
                var saveResult = _menuService.Save(menuToUpdate);
            }
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View(menuToUpdate);
        }
    }

    // GET: MenuController/Edit/5
    public async Task<ActionResult> Edit(int id)
    {
        var item = await _menuService.GetMenuEditAsync(id);
        return View(item);
    }

    // POST: MenuController/Edit/5
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, MenuEditModel item)
    {
        try
        {
            var menuToUpdate = _menuService.GetAllMenuItems().Where(w => w.Id == id).FirstOrDefault();
            if (menuToUpdate != null)
            {
                if (item.Controller == "Page")
                {
                    if (item.ParentId is null)
                    {
                        item.Url = item.Title.ToSlug();
                        item.Argument = item.Title.ToSlug();
                    }
                    else
                    {
                        if (item.ParentId == menuToUpdate.ParentId)
                        {
                            item.Url = $"{menuToUpdate.ParentTitle.ToSlug()}/{item.Title.ToSlug()}";
                            item.Argument = item.Url;
                        }
                    }
                }

                menuToUpdate.Title = item.Title ?? menuToUpdate.Title;
                menuToUpdate.Icon = item.Icon ?? menuToUpdate.Icon;
                menuToUpdate.PageContent = item.PageContent ?? menuToUpdate.PageContent;
                menuToUpdate.Description = item.Description ?? menuToUpdate.Description;
                menuToUpdate.DisplayInNavigation = item.DisplayInNavigation;
                menuToUpdate.DisplayOrder = item.DisplayOrder;
                menuToUpdate.ParentId = item.ParentId;
                menuToUpdate.Url = item.Url;
                menuToUpdate.Controller = item.Controller ?? menuToUpdate.Controller;
                menuToUpdate.Action = item.Action ?? menuToUpdate.Action;
                menuToUpdate.Argument = item.Argument ?? menuToUpdate.Argument;
                var saveResult = _menuService.Save(menuToUpdate);
            }
            return RedirectToAction("Details", "Website", new { id = item.DomainID });
        }
        catch
        {
            return View();
        }
    }

    // GET: MenuController/Delete/5
    public async Task<ActionResult> Delete(int id)
    {
        var item = await _menuService.GetMenuEditAsync(id);
        return View(item);
    }

    // POST: MenuController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id, IFormCollection collection)
    {
        try
        {
            var deleteResult = await _menuService.DeleteMenuAsync(id);

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}
