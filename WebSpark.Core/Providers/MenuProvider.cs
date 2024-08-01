using Microsoft.Data.Sqlite;
using WebSpark.Core.Data;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models.EditModels;

namespace WebSpark.Core.Providers;

public class MenuProvider(WebSparkDbContext webDomainContext) 
    : IMenuProvider, IDisposable, IMenuService
{
    private static List<Models.MenuModel> Create(List<Menu> list)
    {
        if (list == null) return [];
        return [.. list.Select(Create).OrderBy(x => x.Title)];
    }

    private static Models.MenuModel Create(Menu menu)
    {
        if (menu == null) return new Models.MenuModel();

        var item = new Models.MenuModel()
        {
            Id = menu.Id,
            Title = menu.Title,
            Url = menu.Url,
            Icon = menu.Icon,
            DomainID = menu.Domain?.Id ?? 0,
            DomainName = menu.Domain?.Name,
            Description = menu.Description ?? menu.Title,
            Controller = menu.Controller,
            Action = menu.Action?.ToLower(CultureInfo.CurrentCulture) ?? string.Empty,
            Argument = menu.Argument?.ToLower(CultureInfo.CurrentCulture) ?? string.Empty,
            ParentId = menu.Parent?.Id,
            ParentController = menu.Parent?.Controller,
            ParentTitle = menu.Parent?.Title,
            DisplayOrder = menu.DisplayOrder,
            PageContent = menu.PageContent,
            VirtualPath = GetVirtualPath(menu)
        };

        item.Url = string.IsNullOrEmpty(item.Url)
            ? item.VirtualPath.ToLower(CultureInfo.CurrentCulture).Replace(" ", string.Empty)
            : item.Url.ToLower(CultureInfo.CurrentCulture).Replace(" ", string.Empty);

        return item;
    }

    private static string GetVirtualPath(Menu menu)
    {
        return menu.Parent != null
            ? $"{menu.Parent.Action.ToLower(CultureInfo.CurrentCulture)}/{menu.Action.ToLower(CultureInfo.CurrentCulture)}"
            : menu.Action.ToLower(CultureInfo.CurrentCulture);
    }

    private Menu Create(Models.MenuModel menu)
    {
        if (menu == null) return new Menu();
        var dbMenu = new Menu()
        {
            Title = menu.Title,
            Url = menu.Url,
            Icon = menu.Icon,
            Description = menu.Description,
            Controller = menu.Controller,
            Action = menu.Action,
            Argument = menu.Argument,
            DisplayOrder = menu.DisplayOrder,
            PageContent = menu.PageContent,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            UpdatedID = 99,
            CreatedID = 99,
        };
        var dbDomain = webDomainContext.Domain.Where(w => w.Id == menu.DomainID).FirstOrDefault();
        dbMenu.Domain = dbDomain;

        var parentMenu = webDomainContext.Menu.Where(w => w.Id == menu.ParentId).FirstOrDefault();
        if (parentMenu != null)
        {
            dbMenu.Parent = parentMenu;
        }
        return dbMenu;
    }

    private void UpdateDisplayOrder()
    {
        int displayOrder = 10;
        foreach (Menu item in webDomainContext.Menu.OrderBy(o => o.DisplayOrder))
        {
            item.DisplayOrder = displayOrder;
            displayOrder += 10;
        }
        webDomainContext.SaveChanges();
    }

    public bool Delete(int Id)
    {
        var deleteItem = webDomainContext.Menu.Where(w => w.Id == Id).FirstOrDefault();
        if (deleteItem != null)
        {
            webDomainContext.Menu.Remove(deleteItem);
            webDomainContext.SaveChanges();
            return true;
        }
        return false;
    }

    public async Task<MenuEditModel> GetMenuEditAsync(int Id)
    {
        var menuList = webDomainContext.Menu.Include(i => i.Domain).ToList();
        var returnMenu = new MenuEditModel(Create(menuList.Where(w => w.Id == Id).FirstOrDefault()));
        if (returnMenu == null)
            returnMenu = new MenuEditModel();

        returnMenu.Parents = Create(menuList.Where(w => w.Parent == null).ToList()).Select(s => new Models.LookupModel() { Value = s.Id.ToString(), Text = s.Title }).ToList();
        returnMenu.Parents.Insert(0, new Models.LookupModel() { Value = string.Empty, Text = "None" });

        returnMenu.Domains = (await webDomainContext.Set<WebSite>().ToListAsync()).Select(s => new Models.LookupModel() { Value = s.Id.ToString(), Text = s.Name }).ToList();
        return returnMenu;
    }

    public async Task<Models.MenuModel> GetMenuItemAsync(int Id)
    {
        var returnMenu = Create(await webDomainContext.Set<Menu>().Where(w => w.Id == Id).Include(i => i.Domain).FirstOrDefaultAsync());
        if (returnMenu == null)
            returnMenu = new Models.MenuModel();
        return returnMenu;
    }

    public Models.MenuModel GetMenuItem(int Id)
    {
        var returnMenu = Create(webDomainContext.Menu.Where(w => w.Id == Id)
            .Include(i => i.Parent)
            .Include(i => i.Domain)
            .FirstOrDefault());
        returnMenu ??= new Models.MenuModel();

        return returnMenu;
    }

    public IEnumerable<Models.MenuModel> GetMenuList()
    {
        return Create(webDomainContext.Menu.OrderBy(o => o.DisplayOrder)
            .Include(i => i.Parent)
            .Include(i => i.Domain)
            .OrderBy(o => o.DisplayOrder)
            .ToList());
    }

    public List<Models.MenuModel> GetSiteMenu(int DomainId)
    {
        return Create(webDomainContext.Menu.Where(w => w.Domain.Id == DomainId)
            .Include(i => i.Parent)
            .Include(i => i.Domain)
            .OrderBy(o => o.DisplayOrder)
            .ToList());
    }

    public List<Models.MenuModel> Save(List<Models.MenuModel> saveMenus)
    {
        if (saveMenus == null)
        {
            return null;
        }

        var returnMenus = new List<Models.MenuModel>();
        var curMenus = GetMenuList();

        foreach (var menuItem in saveMenus)
        {
            menuItem.ParentId = null;

            var curMenu = curMenus.Where(w => w.DomainID == menuItem.DomainID).Where(w => w.Title == menuItem.Title).FirstOrDefault();
            if (curMenu == null)
            {
                menuItem.Id = curMenu == null ? 0 : curMenu.Id;
                menuItem.ParentId = null;
                Save(menuItem);
            }
        }
        return returnMenus;
    }

    public Models.MenuModel Save(Models.MenuModel saveItem)
    {
        if (saveItem == null)
        {
            return null;
        }

        if (saveItem.Id == 0)
        {
            var saveMenu = Create(saveItem);
            try
            {
                webDomainContext.Menu.Add(saveMenu);
                webDomainContext.SaveChanges();
                saveItem.Id = saveMenu.Id;
            }
            catch
            {
                saveItem.Id = -1;
            }
        }
        else
        {
            try
            {
                var dbMenu = webDomainContext.Menu.Where(w => w.Id == saveItem.Id).FirstOrDefault();
                var parentMenu = webDomainContext.Menu.Where(w => w.Id == saveItem.ParentId).FirstOrDefault();

                if (dbMenu != null)
                {
                    dbMenu.Title = saveItem.Title;
                    dbMenu.Description = saveItem.Description;
                    dbMenu.Controller = saveItem.Controller;
                    dbMenu.Action = saveItem.Action;
                    dbMenu.Argument = saveItem.Argument;
                    dbMenu.Icon = saveItem.Icon;
                    dbMenu.Url = saveItem.Url;
                    dbMenu.PageContent = saveItem.PageContent;
                    dbMenu.DisplayOrder = saveItem.DisplayOrder;
                    dbMenu.Parent = parentMenu;

                    webDomainContext.SaveChanges();
                }
            }
            catch
            {
                saveItem.Id = -1;
            }
        }
        UpdateDisplayOrder();
        return GetMenuItem(saveItem.Id);
    }

    public IEnumerable<Models.MenuModel> GetAllMenuItems()
    {
        return GetMenuList();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        ((IDisposable)webDomainContext).Dispose();
    }

    public async Task<bool> DeleteMenuAsync(int Id)
    {
        var dbMenu = webDomainContext.Menu.Where(w => w.Id == Id).FirstOrDefault();
        if (dbMenu != null)
        {
            try
            {
                var childMenu = webDomainContext.Menu.Where(w => w.Parent.Id == Id);
                foreach (var child in childMenu)
                {
                    child.Parent = null;
                }
                webDomainContext.Menu.Remove(dbMenu);
                await webDomainContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }

    // New methods to handle keywords and content parts
    public async Task AddKeywordToMenuAsync(int menuId, int keywordId)
    {
        var menu = await webDomainContext.Menu.FindAsync(menuId);
        var keyword = await webDomainContext.Keywords.FindAsync(keywordId);

        if (menu != null && keyword != null)
        {
            menu.Keywords.Add(keyword);
            await webDomainContext.SaveChangesAsync();
        }
    }

    public async Task AddContentPartToMenuAsync(int menuId, int contentPartId)
    {
        var menu = await webDomainContext.Menu.FindAsync(menuId);
        var contentPart = await webDomainContext.ContentParts.FindAsync(contentPartId);

        if (menu != null && contentPart != null)
        {
            foreach (var keyword in contentPart.Keywords)
            {
                if (!menu.Keywords.Contains(keyword))
                {
                    menu.Keywords.Add(keyword);
                }
            }
            await webDomainContext.SaveChangesAsync();
        }
    }

    public async Task<List<ContentPart>> GetContentPartsForMenuAsync(int menuId)
    {
        var menu = await webDomainContext.Menu.Include(m => m.Keywords).FirstOrDefaultAsync(m => m.Id == menuId);

        if (menu != null)
        {
            var contentParts = await webDomainContext.ContentParts
                .Include(cp => cp.Keywords)
                .Where(cp => cp.Keywords.Any(k => menu.Keywords.Contains(k)))
                .ToListAsync();

            return contentParts;
        }
        return new List<ContentPart>();
    }

    public async Task<Keyword> AddKeywordAsync(string name, string description)
    {
        var keyword = new Keyword
        {
            Name = name,
            Description = description
        };
        webDomainContext.Keywords.Add(keyword);
        await webDomainContext.SaveChangesAsync();
        return keyword;
    }

    public async Task<ContentPart> AddContentPartAsync(string title, string description, string content)
    {
        var contentPart = new ContentPart
        {
            Title = title,
            Description = description,
            Content = content
        };
        webDomainContext.ContentParts.Add(contentPart);
        await webDomainContext.SaveChangesAsync();
        return contentPart;
    }

    public async Task AssociateContentPartWithKeywordAsync(int contentPartId, int keywordId)
    {
        var contentPart = await webDomainContext.ContentParts.FindAsync(contentPartId);
        var keyword = await webDomainContext.Keywords.FindAsync(keywordId);

        if (contentPart != null && keyword != null)
        {
            contentPart.Keywords.Add(keyword);
            await webDomainContext.SaveChangesAsync();
        }
    }
}
