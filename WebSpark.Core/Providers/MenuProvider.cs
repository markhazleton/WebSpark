using Microsoft.Data.Sqlite;
using WebSpark.Core.Data;
using WebSpark.Domain.EditModels;
using WebSpark.Domain.Entities;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;

namespace WebSpark.Core.Providers;

/// <summary>
/// Class DbMenu.
/// Implements the <see cref="IMenuProvider" />
/// </summary>
/// <seealso cref="IMenuProvider" />
public class MenuProvider : IMenuProvider, IDisposable, IMenuService
{
    private readonly WebSparkDbContext _context;
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuProvider" /> class.
    /// </summary>
    public MenuProvider(WebSparkDbContext webDomainContext)
    {
        _context = webDomainContext;
    }

    /// <summary>
    /// Creates the specified list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    private List<MenuModel> Create(List<Menu> list)
    {
        if (list == null) return [];
        return [.. list.Select(item => Create(item)).OrderBy(x => x.Title)];
    }

    /// <summary>
    /// Creates the specified menu.
    /// </summary>
    /// <param name="menu">The menu.</param>
    /// <returns>MenuModel.</returns>
    private static MenuModel Create(Menu menu)
    {
        if (menu == null) return new MenuModel();

        var item = new MenuModel()
        {
            Id = menu.Id,
            Title = menu.Title,
            Url = menu.Url,
            Icon = menu.Icon,
            DomainID = menu?.Domain?.Id ?? 0,
            DomainName = menu?.Domain?.Name,
            Description = menu.Description ?? menu.Title,
            Controller = menu.Controller,
            Action = menu.Action?.ToLower(CultureInfo.CurrentCulture) ?? string.Empty,
            Argument = menu.Argument?.ToLower(CultureInfo.CurrentCulture) ?? string.Empty,
            ParentId = menu.Parent != null ? menu.Parent?.Id : null,
            ParentController = menu.Parent != null ? menu.Parent?.Controller : string.Empty,
            ParentTitle = menu.Parent != null ? menu.Parent?.Title : string.Empty,
            DisplayOrder = menu.DisplayOrder,
            PageContent = menu.PageContent,
            VirtualPath = GetVirualPath(menu)
        };

        if (string.IsNullOrEmpty(item.Url))
        {
            item.Url = item.VirtualPath.ToLower(CultureInfo.CurrentCulture).Replace(" ", string.Empty);
        }
        else
        {
            item.Url = item.Url.ToLower(CultureInfo.CurrentCulture).Replace(" ", string.Empty);
        }
        return item;
    }

    private static string GetVirualPath(Menu menu)
    {
        return menu.Parent != null ?
            $"{menu.Parent.Action.ToLower(CultureInfo.CurrentCulture)}/{menu.Action.ToLower(CultureInfo.CurrentCulture)}" :
            menu.Action.ToLower(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Creates the specified menu.
    /// </summary>
    /// <param name="menu">The menu.</param>
    /// <returns>Menu.</returns>
    private Menu Create(MenuModel menu)
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
        var dbDomain = _context.Domain.Where(w => w.Id == menu.DomainID).FirstOrDefault();
        dbMenu.Domain = dbDomain;

        var parentMenu = _context.Menu.Where(w => w.Id == menu.ParentId).FirstOrDefault();
        if (parentMenu != null)
        {
            dbMenu.Parent = parentMenu;
        }
        return dbMenu;
    }

    /// <summary>
    /// Updates the display order.
    /// </summary>
    private void UpdateDisplayOrder()
    {
        int Display = 10;
        foreach (Menu item in _context.Menu.OrderBy(o => o.DisplayOrder))
        {
            item.DisplayOrder = Display;
            Display += 10;
        }
        _context.SaveChanges();
    }

    /// <summary>
    /// Deletes the specified identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public bool Delete(int Id)
    {
        var deleteItem = _context.Menu.Where(w => w.Id == Id).FirstOrDefault();
        if (deleteItem != null)
        {
            _context.Menu.Remove(deleteItem);
            _context.SaveChanges();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public async Task<MenuEditModel> GetMenuEditAsync(int Id)
    {
        var menuList = _context.Menu.Include(i => i.Domain).ToList();
        var returnMenu = new MenuEditModel(Create(menuList.Where(w => w.Id == Id).FirstOrDefault()));
        if (returnMenu == null)
            returnMenu = new MenuEditModel();

        returnMenu.Parents = Create(menuList.Where(w => w.Parent == null).ToList()).Select(s => new LookupModel() { Value = s.Id.ToString(), Text = s.Title }).ToList();
        returnMenu.Parents.Insert(0, new LookupModel() { Value = string.Empty, Text = "None" });

        returnMenu.Domains = (await _context.Set<WebSite>().ToListAsync()).Select(s => new LookupModel() { Value = s.Id.ToString(), Text = s.Name }).ToList();
        return returnMenu;
    }
    public async Task<MenuModel> GetMenuItemAsync(int Id)
    {
        var returnMenu = Create(await _context.Set<Menu>().Where(w => w.Id == Id).Include(i => i.Domain).FirstOrDefaultAsync());
        if (returnMenu == null)
            returnMenu = new MenuModel();
        return returnMenu;
    }


    /// <summary>
    /// Gets the menu by identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>MenuModel.</returns>
    public MenuModel GetMenuItem(int Id)
    {
        var returnMenu = Create(_context.Menu.Where(w => w.Id == Id)
            .Include(i => i.Parent)
            .Include(i => i.Domain)
            .FirstOrDefault());
        if (returnMenu == null)
            returnMenu = new MenuModel();

        return returnMenu;
    }

    /// <summary>
    /// Gets the menu list.
    /// </summary>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    public IEnumerable<MenuModel> GetMenuList()
    {
        return Create([.. _context.Menu.OrderBy(o => o.DisplayOrder)
            .Include(i => i.Parent)
            .Include(i => i.Domain)
            .OrderBy(o => o.DisplayOrder)]);
    }

    /// <summary>
    /// Gets the site menu.
    /// </summary>
    /// <param name="DomainId">The domain identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    public List<MenuModel> GetSiteMenu(int DomainId)
    {
        return Create([.. _context.Menu.Where(w => w.Domain.Id == DomainId)
            .Include(i => i.Parent)
            .Include(i => i.Domain)
            .OrderBy(o => o.DisplayOrder)]);
    }


    public List<MenuModel> Save(List<MenuModel> saveMenus)
    {
        if (saveMenus == null)
        {
            return null;
        }

        var returnMenus = new List<MenuModel>();
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


    /// <summary>
    /// Saves the specified save item.
    /// </summary>
    /// <param name="saveItem">The save item.</param>
    /// <returns>MenuModel.</returns>
    public MenuModel Save(MenuModel saveItem)
    {
        if (saveItem == null)
        {
            return null;
        }

        if (saveItem?.Id == 0)
        {
            var saveMenu = Create(saveItem);
            try
            {
                _context.Menu.Add(saveMenu);
                _context.SaveChanges();
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
                var dbMenu = _context.Menu.Where(w => w.Id == saveItem.Id).FirstOrDefault();
                var parentMenu = _context.Menu.Where(w => w.Id == saveItem.ParentId).FirstOrDefault();

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
                    if (parentMenu is null)
                    {
                        dbMenu.Parent = null;
                    }
                    else
                    {
                        dbMenu.Parent = parentMenu;
                    }


                    _context.SaveChanges();
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
    public IEnumerable<MenuModel> GetAllMenuItems()
    {
        return GetMenuList();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        ((IDisposable)_context).Dispose();
    }

    public async Task<bool> DeleteMenuAsync(int Id)
    {
        var dbMenu = _context.Menu.Where(w => w.Id == Id).FirstOrDefault();
        if (dbMenu != null)
        {
            try
            {
                var childMenu = _context.Menu.Where(w => w.Parent.Id == Id);
                foreach (var child in childMenu)
                {
                    child.Parent = null;
                }
                _context.Menu.Remove(dbMenu);
                await _context.SaveChangesAsync();
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
}
