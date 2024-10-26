using WebSpark.Core.Extensions;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models.EditModels;

namespace WebSpark.Portal.Areas.WebCMS.Controllers;

public class MenuController(
    ILogger<MenuController> _logger,
    IScopeInformation _scopeInfo,
    IMenuService menuService) : WebCMSBaseController
{
    // GET: MenuController
    public ActionResult Index()
    {
        return View(menuService.GetAllMenuItems());
    }

    // GET: MenuController/Details/5
    public async Task<ActionResult> Details(int id)
    {
        return View(await menuService.GetMenuEditAsync(id));
    }

    // GET: MenuController/Create
    public async Task<ActionResult> Create()
    {
        var item = await menuService.GetMenuEditAsync(0);
        return View(item);
    }

    // POST: MenuController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(MenuEditModel item)
    {
        var menuToUpdate = new Core.Models.MenuModel();
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
                var saveResult = menuService.Save(menuToUpdate);
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
        var item = await menuService.GetMenuEditAsync(id);
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
            var menuToUpdate = menuService.GetAllMenuItems().Where(w => w.Id == id).FirstOrDefault();
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
                var saveResult = menuService.Save(menuToUpdate);
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
        var item = await menuService.GetMenuEditAsync(id);
        return View(item);
    }

    // POST: MenuController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id, IFormCollection collection)
    {
        try
        {
            var deleteResult = await menuService.DeleteMenuAsync(id);

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}
