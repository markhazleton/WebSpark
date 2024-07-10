using Microsoft.Extensions.DependencyInjection;
using WebSpark.Core.Data;
using WebSpark.Core.Infrastructure;
using WebSpark.Domain.EditModels;
using WebSpark.Domain.Entities;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;
using WebSpark.Domain.ViewModels;
using WebSpark.RecipeManager.Entities;
using WebSpark.RecipeManager.Models;

namespace WebSpark.Core.Providers;

public class WebsiteServiceFactory(IServiceProvider serviceProvider) : IWebsiteServiceFactory
{
    public IWebsiteService Create()
    {
        return serviceProvider.GetRequiredService<IWebsiteService>();
    }
}


/// <summary>
///  Domain
/// </summary>
public class WebsiteProvider(WebSparkDbContext webDomainContext) : IWebsiteService, IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// Returns Website Model from Website table
    /// </summary>
    /// <param name="website"></param>
    /// <returns></returns>
    private WebsiteModel Create(WebSite website)
    {
        if (website == null)
        {
            return new WebsiteModel();
        }

        var item = new WebsiteModel()
        {
            Id = website.Id,
            Name = website.Name,
            SiteStyle = website.Style,
            Description = website.Description,
            SiteTemplate = website.Template,
            WebsiteTitle = website.Title,
            WebsiteUrl = website.DomainUrl,
            SiteName = website.GalleryFolder,
            UseBreadCrumbURL = website.UseBreadCrumbUrl,
            VersionNo = website.VersionNo,
            Menu = Create(website.Menus, false),
            ModifiedDT = website.UpdatedDate,
            ModifiedID = website.UpdatedID ?? 99,
        };

        if (website.Id == 2) item.Menu.AddRange(CreateRecipeMenu());

        return item;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private List<RecipeModel> Create(IEnumerable<Recipe> list)
    {
        return list == null ? [] : [.. list.Select(Create).OrderBy(x => x.Name)];
    }

    /// <summary>
    /// Creates the specified recipe.
    /// </summary>
    /// <param name="Recipe">The recipe.</param>
    /// <returns>RecipeModel.</returns>
    private RecipeModel Create(Recipe Recipe)
    {
        return Recipe == null
            ? new RecipeModel()
            : new RecipeModel()
            {
                RecipeURL = FormatHelper.GetRecipeURL(Recipe.Name),
                Id = Recipe.Id,
                Name = Recipe.Name,
                Ingredients = Recipe.Ingredients,
                Instructions = Recipe.Instructions,
                Description = string.IsNullOrEmpty(Recipe.Description) ? Recipe.Name : Recipe.Description,
                Servings = Recipe.Servings,
                AuthorNM = Recipe.AuthorName,
                AverageRating = Recipe.AverageRating,
                IsApproved = Recipe.IsApproved,
                CommentCount = 0,
                RecipeCategory = Create(Recipe.RecipeCategory),
                RecipeCategoryID = Recipe.RecipeCategory.Id,
                RecipeCategoryNM = Recipe.RecipeCategory.Name,
                RatingCount = Recipe.RatingCount,
                ViewCount = Recipe.ViewCount,
                LastViewDT = Recipe.LastViewDt,
            };
    }

    /// <summary>
    /// Returns Domain Model from Domain table
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    private static WebSite Create(WebsiteModel domain)
    {
        var item = new WebSite()
        {
            Id = domain.Id,
            Name = domain.Name,
            Description = domain.Description,
            Template = domain.SiteTemplate,
            Style = domain.SiteStyle,
            Title = domain.WebsiteTitle,
            DomainUrl = domain.WebsiteUrl,
            GalleryFolder = domain.SiteName,
            UseBreadCrumbUrl = domain.UseBreadCrumbURL,
            VersionNo = domain.VersionNo
        };
        return item;
    }

    /// <summary>
    /// Creates the specified list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    private List<WebsiteModel> Create(List<WebSite> list)
    {
        return list == null ? [] : [.. list.Select(Create).OrderBy(x => x.Name)];
    }
    /// <summary>
    /// Creates the specified rc.
    /// </summary>
    /// <param name="rc">The rc.</param>
    /// <returns>RecipeCategoryModel.</returns>
    private RecipeCategoryModel Create(RecipeCategory rc, bool LoadRecipes = false)
    {
        return rc == null
            ? new RecipeCategoryModel()
            : new RecipeCategoryModel()
            {
                DisplayOrder = rc.DisplayOrder,
                IsActive = rc.IsActive,
                Description = rc.Comment,
                Id = rc.Id,
                Name = rc.Name,
                Url = FormatHelper.GetRecipeCategoryURL(rc.Name),
                Recipes = LoadRecipes ? Create(rc.Recipe.ToList()) : []
            };
    }
    private static List<MenuModel> Create(ICollection<Menu> list, bool LoadChild = false)
    {
        var menuList = list == null ? [] : list.Select(item => Create(item, LoadChild)).OrderBy(x => x.Title).ToList();
        foreach (var menu in menuList.Where(w => w.ParentId is null).OrderBy(o => o.DisplayOrder))
        {
            menu.IsHomePage = true;
            menu.Url = "/";
            break;

        };

        return menuList;
    }
    /// <summary>
    /// Creates the specified menu.
    /// </summary>
    /// <param name="menu">The menu.</param>
    /// <returns>MenuModel.</returns>
    private static MenuModel Create(Menu menu, bool LoadChild = false)
    {
        if (menu == null)
        {
            return new MenuModel();
        }

        var item = new MenuModel()
        {
            Id = menu.Id,
            Title = menu.Title,
            Url = menu.Url,
            Icon = menu.Icon,
            DomainID = menu.Domain != null ? menu.Domain.Id : 0,
            DisplayInNavigation = true,
            Description = string.IsNullOrEmpty(menu.Description) ? menu.Title : menu.Description,
            Controller = menu.Controller,
            Action = menu.Action?.ToLower(CultureInfo.CurrentCulture),
            Argument = menu.Argument?.ToLower(CultureInfo.CurrentCulture),
            ParentId = menu.Parent?.Id,
            ParentController = menu.Parent != null ? menu.Parent.Controller : string.Empty,
            ParentTitle = menu.Parent != null ? menu.Parent.Title : string.Empty,
            DisplayOrder = menu.DisplayOrder,
            PageContent = Markdig.Markdown.ToHtml(menu.PageContent),
            VirtualPath = menu.Parent != null ? ($"{menu.Parent.Action.ToLower(CultureInfo.CurrentCulture)}/{menu.Action.ToLower(CultureInfo.CurrentCulture)}") : menu.Action.ToLower(CultureInfo.CurrentCulture)
        };

        if (string.IsNullOrEmpty(item.Url))
        {
            item.Url = item.VirtualPath.ToLower(CultureInfo.CurrentCulture).Replace(" ", string.Empty);
        }
        else
        {
            item.Url = item.Url.ToLower(CultureInfo.CurrentCulture).Replace(" ", string.Empty);
        }
        item.Url = $"/{item.Url}";
        return item;
    }

    private WebsiteVM CreateBaseView(WebSite domain)
    {
        if (domain == null)
        {
            return new WebsiteVM();
        }

        var siteURI = ValidateUrl(domain.DomainUrl);

        var item = new WebsiteVM()
        {
            WebsiteId = domain.Id,
            WebsiteName = domain.Name,
            WebsiteStyle = domain.Style,
            CurrentStyle = domain.Style,
            SiteName = domain.GalleryFolder,
            Template = domain.Template,
            MetaDescription = domain.Description,
            MetaKeywords = "TODO",
            PageTitle = domain.Title,
            SiteUrl = siteURI,
            Menu = Create(domain.Menus, false),
        };

        if (domain.Id == 2) item.Menu.AddRange(CreateRecipeMenu());

        return item;
    }

    /// <summary>
    /// Creates the website menu.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="DomainID">The website identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    private List<MenuModel> CreateRecipeMenu()
    {
        var categoryList = webDomainContext.RecipeCategory.ToList();
        List<MenuModel> categoryMenu = categoryList == null ? [] : [.. categoryList.Select(GetMenuItem).OrderBy(x => x.DisplayOrder)];

        var list = webDomainContext.Recipe.Include(i => i.RecipeCategory).ToList();
        categoryMenu.AddRange(list == null ? [] : [.. list.Select(GetMenuItem).OrderBy(x => x.DisplayOrder)]);

        return categoryMenu;
    }


    private MenuModel GetMenuItem(RecipeCategory category)
    {
        return category == null
            ? new MenuModel()
            : new MenuModel()
            {
                ParentId = category?.Id,
                Controller = "recipe",
                Action = "Category",
                Argument = FormatHelper.GetSafePath(category.Name),
                Description = string.IsNullOrEmpty(category.Comment) ? category.Name : category.Comment,
                Title = category.Name,
                ParentController = "recipe",
                ParentTitle = "Recipe",
                Url = FormatHelper.GetRecipeCategoryURL(category.Name),
                VirtualPath = FormatHelper.GetRecipeCategoryURL(category.Name),
                Icon = "fa fa-food",
                PageContent = category.Comment,
                DisplayOrder = 10,
                DisplayInNavigation = false
            };
    }


    /// <summary>
    /// Gets the menu item.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <param name="domain">The website.</param>
    /// <returns>MenuModel.</returns>
    private MenuModel GetMenuItem(Recipe recipe)
    {
        return recipe == null
            ? new MenuModel()
            : new MenuModel()
            {
                Id = recipe.Id,
                ParentId = recipe.RecipeCategory?.Id,
                Controller = "recipe",
                Action = "Index",
                Argument = FormatHelper.GetSafePath(recipe.Name),
                Description = string.IsNullOrEmpty(recipe.Description) ? recipe.Name : recipe.Description,
                Title = recipe.Name,
                ParentController = "recipe",
                ParentTitle = "Recipe",
                Url = FormatHelper.GetRecipeURL(recipe.Name),
                VirtualPath = FormatHelper.GetRecipeURL(recipe.Name),
                Icon = "fa fa-food",
                PageContent = recipe.Description,
                DisplayOrder = 100,
                DisplayInNavigation = false
            };
    }


    public bool Delete(int Id)
    {
        if (Id == 0)
        {
            return false;
        }

        var deleteItem = webDomainContext.Domain.Where(w => w.Id == Id).FirstOrDefault();
        if (deleteItem != null)
        {
            webDomainContext.Domain.Remove(deleteItem);
            webDomainContext.SaveChanges();
            return true;
        }
        return false;
    }

    public List<WebsiteModel> Get()
    {
        return Create(webDomainContext.Domain.OrderBy(o => o.Name).ToList());
    }

    public async Task<WebsiteModel> GetAsync(int id)
    {
        var returnMenu = Create(await webDomainContext.Set<WebSite>()
            .Where(w => w.Id == id)
            .Include(i => i.Menus).FirstOrDefaultAsync());
        if (returnMenu == null)
            returnMenu = new WebsiteModel();
        return returnMenu;
    }

    /// <summary>
    /// Get Base View By Host Name
    /// </summary>
    /// <param name="host"></param>
    /// <param name="defaultSiteId"></param>
    /// <returns></returns>
    public async Task<WebsiteVM> GetBaseViewByHostAsync(string host, string defaultSiteId = null)
    {
        var bvm = CreateBaseView(await webDomainContext.Domain.Where(w => w.DomainUrl.ToLower().Contains(host.ToLower()))
            .Include(i => i.Menus).FirstOrDefaultAsync());

        if (bvm.WebsiteId == 0)
        {
            bvm = CreateBaseView(await webDomainContext.Domain.Where(w => host.ToLower().Contains(w.Name.ToLower()))
                .Include(i => i.Menus).FirstOrDefaultAsync());
        }

        if (bvm.WebsiteId == 0)
        {
            int siteId = 0;
            if (!int.TryParse(defaultSiteId ?? "1", out siteId))
                siteId = 1;

            bvm = CreateBaseView(await webDomainContext.Domain.Where(w => w.Id == siteId)
                .Include(i => i.Menus).FirstOrDefaultAsync());

            if (bvm.WebsiteId == 0)
            {
                siteId = await webDomainContext.Domain.Select(s => s.Id).FirstOrDefaultAsync();
                bvm = CreateBaseView(await webDomainContext.Domain.Where(w => w.Id == siteId)
                    .Include(i => i.Menus).FirstOrDefaultAsync());
            }
        }
        return bvm;
    }

    public async Task<WebsiteVM> GetBaseViewModelAsync(int id)
    {
        var bvm = CreateBaseView(await webDomainContext.Domain.Where(w => w.Id == id)
            .Include(i => i.Menus).FirstOrDefaultAsync());

        return bvm;
    }
    public async Task<WebsiteEditModel> GetEditAsync(int id)
    {
        var website = new WebsiteEditModel(Create(await webDomainContext.Set<WebSite>()
            .Where(w => w.Id == id)
            .Include(i => i.Menus).FirstOrDefaultAsync()));
        return website ??= new WebsiteEditModel();
    }

    public WebsiteModel Save(WebsiteModel saveItem)
    {
        if (saveItem == null)
        {
            return null;
        }

        if (saveItem?.Id == 0)
        {
            var saveWebsite = Create(saveItem);
            try
            {
                webDomainContext.Domain.Add(saveWebsite);
                webDomainContext.SaveChanges();
                saveItem.Id = saveWebsite.Id;
            }
            catch (Exception ex)
            {
                saveItem.Id = -1;
                saveItem.Message = ex.Message;
                return saveItem;
            }
        }
        else
        {
            try
            {
                var dbWebsite = webDomainContext.Domain.Where(w => w.Id == saveItem.Id).FirstOrDefault();
                if (dbWebsite != null)
                {
                    dbWebsite.Name = saveItem.Name;
                    dbWebsite.Description = saveItem.Description;
                    dbWebsite.Style = saveItem.SiteStyle;
                    dbWebsite.Template = saveItem.SiteTemplate;
                    dbWebsite.Title = saveItem.WebsiteTitle;
                    dbWebsite.DomainUrl = saveItem.WebsiteUrl;
                    dbWebsite.GalleryFolder = saveItem.SiteName;
                    dbWebsite.UseBreadCrumbUrl = saveItem.UseBreadCrumbURL;
                    dbWebsite.UpdatedID = saveItem.ModifiedID;
                    dbWebsite.VersionNo = dbWebsite.VersionNo++;
                    dbWebsite.UpdatedDate = DateTime.UtcNow;
                    webDomainContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                saveItem.Id = -1;
                saveItem.Message = ex.Message;
                return saveItem;
            }
        }
        return Create(webDomainContext.Domain.Where(w => w.Id == saveItem.Id).FirstOrDefault());
    }

    public static Uri ValidateUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return uriResult;
        }
        else
        {
            return null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (webDomainContext != null)
                {
                    webDomainContext.Dispose();
                    webDomainContext = null;
                }
            }
            disposedValue = true;
        }
    }

    ~WebsiteProvider()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
