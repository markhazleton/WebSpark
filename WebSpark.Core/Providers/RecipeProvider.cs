using WebSpark.Core.Data;
using WebSpark.Core.Infrastructure;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models.ViewModels;

namespace WebSpark.Core.Providers;

/// <summary>
/// Recipe Service
/// Implements the <see cref="WebSparkDbContext" />
/// Implements the <see cref="IMenuProvider" />
/// </summary>
/// <seealso cref="WebSparkDbContext" />
/// <seealso cref="IMenuProvider" />
public class RecipeProvider(WebSparkDbContext webDomainContext) : Interfaces.IMenuProvider, Interfaces.IRecipeService, IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private List<Models.RecipeModel> Create(List<Recipe> list)
    {
        if (list == null) return [];
        return [.. list.Select(Create).OrderBy(x => x.Name)];
    }

    /// <summary>
    /// Creates the specified list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>List&lt;RecipeCategoryModel&gt;.</returns>
    private List<Models.RecipeCategoryModel> Create(List<RecipeCategory> list)
    {
        if (list == null) return [];
        return [.. list.Select(item => Create(item)).OrderBy(x => x.Name)];
    }

    /// <summary>
    /// Creates the specified recipe.
    /// </summary>
    /// <param name="Recipe">The recipe.</param>
    /// <returns>RecipeModel.</returns>
    private Models.RecipeModel Create(Recipe Recipe)
    {
        if (Recipe == null) return new Models.RecipeModel();

        return new Models.RecipeModel()
        {
            RecipeURL = FormatHelper.GetRecipeURL(Recipe.Name),
            Id = Recipe.Id,
            Name = Recipe.Name,
            Ingredients = Recipe?.Ingredients ?? Recipe.Name,
            Instructions = Recipe?.Instructions ?? Recipe.Name,
            Description = Recipe?.Description ?? Recipe.Name,
            SEO_Keywords = Recipe?.Keywords ?? Recipe.Name,
            Servings = Recipe.Servings,
            AuthorNM = Recipe.AuthorName,
            AverageRating = Recipe.AverageRating,
            IsApproved = Recipe.IsApproved,
            CommentCount = 0,
            RecipeCategory = Create(Recipe.RecipeCategory),
            RecipeCategoryID = Recipe?.RecipeCategory?.Id ?? 0,
            RatingCount = Recipe.RatingCount,
            ViewCount = Recipe.ViewCount,
            LastViewDT = Recipe.LastViewDt,
        };
    }

    /// <summary>
    /// Creates the specified recipe.
    /// </summary>
    /// <param name="Recipe">The recipe.</param>
    /// <returns>Recipe.</returns>
    private Recipe Create(Models.RecipeModel Recipe)
    {
        if (Recipe == null) return new Recipe();

        var Category = webDomainContext.RecipeCategory.Where(w => w.Id == Recipe.RecipeCategoryID).FirstOrDefault();
        var Domain = webDomainContext.Domain.Where(w => w.Id == Recipe.DomainID).FirstOrDefault();

        return new Recipe()
        {
            Id = Recipe.Id,
            Name = Recipe.Name,
            Ingredients = Recipe.Ingredients,
            Instructions = Recipe.Instructions,
            Keywords = Recipe.SEO_Keywords,
            Description = string.IsNullOrEmpty(Recipe.Description) ? Recipe.Name : Recipe.Description,
            AuthorName = Recipe.AuthorNM,
            AverageRating = Recipe.AverageRating,
            IsApproved = Recipe.IsApproved,
            CommentCount = Recipe.CommentCount,
            RatingCount = Recipe.RatingCount,
            ViewCount = Recipe.ViewCount,
            LastViewDt = Recipe.LastViewDT,
            RecipeCategory = Category,
            Domain = Domain,
            Servings = Recipe.Servings,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };
    }
    private static RecipeCategory Create(Models.RecipeCategoryModel s)
    {
        return new RecipeCategory()
        {
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            Comment = s.Name,
            Id = s.Id,
            Name = s.Name
        };
    }

    private List<Models.RecipeImageModel> Create(List<RecipeImage> dbRecipeImage)
    {
        var recipeImageList = new List<Models.RecipeImageModel>();
        foreach (var dbItem in dbRecipeImage)
        {
            recipeImageList.Add(Create(dbItem));
        }
        return recipeImageList;
    }

    private Models.RecipeImageModel Create(RecipeImage dbItem)
    {
        var recipeImage = new Models.RecipeImageModel()
        {
            Id = dbItem.Id,
            Recipe = Create(dbItem.Recipe),
            DisplayOrder = dbItem.DisplayOrder,
            FileDescription = dbItem.FileDescription,
            FileName = dbItem.FileName,
        };
        return recipeImage;
    }

    /// <summary>
    /// Creates the specified rc.
    /// </summary>
    /// <param name="rc">The rc.</param>
    /// <returns>RecipeCategoryModel.</returns>
    private Models.RecipeCategoryModel Create(RecipeCategory rc, bool LoadRecipes = false)
    {
        if (rc == null) return new Models.RecipeCategoryModel();

        return new Models.RecipeCategoryModel()
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
    /// <summary>
    /// Creates the domain menu.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="DomainID">The domain identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    private List<Models.MenuModel> CreateDomainMenu(List<Recipe> list, int DomainID)
    {
        if (list == null) return [];

        if (DomainID > 0)
        {
            var webSite = webDomainContext.Domain.Where(w => w.Id == DomainID).FirstOrDefault();
            var page = webDomainContext.Menu.Where(w => w.Id == DomainID).Where(w => w.Title == "Recipe").FirstOrDefault();

            if (webSite != null)
            {
                if (page == null)
                {
                    return [.. list.Select(item => GetMenuItem(item, webSite)).OrderBy(x => x.DisplayOrder)];
                }
                else
                {
                    return [.. list.Select(item => GetMenuItem(item, webSite, page)).OrderBy(x => x.DisplayOrder)];
                }
            }
            else
            {
                return [.. list.Select(item => GetMenuItem(item)).OrderBy(x => x.DisplayOrder)];
            }
        }
        return [];

    }


    /// <summary>
    /// Creates the menu.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    private static List<Models.MenuModel> CreateMenu(List<Recipe> list)
    {
        if (list == null) return [];
        return [.. list.Select(item => GetMenuItem(item)).OrderBy(x => x.DisplayOrder)];
    }


    /// <summary>
    /// Gets the menu item.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <returns>MenuModel.</returns>
    private static Models.MenuModel GetMenuItem(Recipe recipe)
    {
        if (recipe == null) return new Models.MenuModel();

        return new Models.MenuModel()
        {
            Id = recipe.Id,
            ParentId = recipe.RecipeCategory?.Id,
            Controller = "Recipe",
            Action = "index",
            Description = recipe.Name,
            KeyWords = recipe.Keywords,
            Title = recipe.Name,
            Url = FormatHelper.GetRecipeURL(recipe.Name),
            DisplayInNavigation = false
        };

    }

    /// <summary>
    /// Gets the menu item.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <param name="domain">The domain.</param>
    /// <returns>MenuModel.</returns>
    private static Models.MenuModel GetMenuItem(Recipe recipe, WebSite domain)
    {
        if (recipe == null) return new Models.MenuModel();

        return new Models.MenuModel()
        {
            Id = recipe.Id,
            ParentId = recipe.RecipeCategory?.Id,
            Controller = "recipe",
            Action = "index",
            Description = recipe.Description,
            Title = recipe.Name,
            DomainID = domain.Id,
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
    /// <summary>
    /// Gets the menu item.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <param name="domain">The domain.</param>
    /// <param name="page">The page.</param>
    /// <returns>MenuModel.</returns>
    private static Models.MenuModel GetMenuItem(Recipe recipe, WebSite domain, Menu page)
    {
        if (recipe == null) return new Models.MenuModel();

        return new Models.MenuModel()
        {
            Id = recipe.Id,
            ParentId = page.Id,
            Controller = "recipe",
            ParentController = "recipe",
            ParentTitle = "recipe",
            Action = "index",
            Description = recipe.Description,
            Title = recipe.Name,
            Url = FormatHelper.GetRecipeURL(recipe.Name),
            DomainID = domain.Id,
            VirtualPath = FormatHelper.GetRecipeURL(recipe.Name),
            Icon = "fa fa-food",
            PageContent = recipe.Description,
            DisplayOrder = 100,
            DisplayInNavigation = false
        };
    }

    /// <summary>
    /// Deletes the specified identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public bool Delete(int Id)
    {
        var deleteItem = webDomainContext.Recipe.Where(w => w.Id == Id).FirstOrDefault();
        if (deleteItem != null)
        {
            webDomainContext.Recipe.Remove(deleteItem);
            webDomainContext.SaveChanges();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the recipe list.
    /// </summary>
    /// <returns>List&lt;RecipeModel&gt;.</returns>
    public IEnumerable<Models.RecipeModel> Get()
    {
        var theList = webDomainContext.Recipe.Include(r => r.RecipeCategory).Include(i => i.RecipeImage).ToList();
        return Create(theList);

        // return Create(_context.Recipe.Include(r => r.RecipeCategory).ToList());
    }

    /// <summary>
    /// Gets the by identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>RecipeModel.</returns>
    public Models.RecipeModel Get(int Id)
    {
        var returnRecipe = Create(webDomainContext.Recipe.Where(w => w.Id == Id).Include(r => r.RecipeCategory).FirstOrDefault());
        returnRecipe.RecipeCategories = webDomainContext.RecipeCategory.Select(s => new Models.LookupModel() { Value = s.Id.ToString(), Text = s.Name }).ToList();
        return returnRecipe;
    }


    /// <summary>
    /// Gets the menu list.
    /// </summary>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    public List<Models.MenuModel> GetAllMenuItems()
    {
        return CreateMenu([.. webDomainContext.Recipe]);
    }

    /// <summary>
    /// Get Menu Item for a Recipe Id
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>MenuModel.</returns>
    public Models.MenuModel GetMenuItem(int Id)
    {
        return GetMenuItem(webDomainContext.Recipe.Where(w => w.Id == Id).FirstOrDefault());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public async Task<Models.MenuModel> GetMenuItemAsync(int Id)
    {
        var returnMenu = GetMenuItem(await webDomainContext.Set<Recipe>().Where(w => w.Id == Id).FirstOrDefaultAsync());
        if (returnMenu == null)
            returnMenu = new Models.MenuModel();
        return returnMenu;
    }

    public IEnumerable<Models.MenuModel> GetMenuList()
    {
        return new List<Models.MenuModel>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public Models.RecipeCategoryModel GetRecipeCategoryById(int Id)
    {
        var returnCategory = new Models.RecipeCategoryModel();
        returnCategory = Create(webDomainContext.RecipeCategory.Include(i => i.Recipe).Where(w => w.Id == Id).FirstOrDefault(), LoadRecipes: true);
        return returnCategory;
    }

    /// <summary>
    /// Gets the recipe category list.
    /// </summary>
    /// <returns>List&lt;RecipeCategoryModel&gt;.</returns>
    public List<Models.RecipeCategoryModel> GetRecipeCategoryList()
    {
        return Create(webDomainContext.RecipeCategory.ToList());
    }

    public List<Models.RecipeImageModel> GetRecipeImages()
    {
        var dbRecipeImage = webDomainContext.RecipeImage.Include(i => i.Recipe).ToList();
        return Create(dbRecipeImage);
    }

    public RecipeVM GetRecipeVMHostAsync(string host, WebsiteVM baseVM)
    {
        var recipeVM = new RecipeVM(baseVM)
        {
            CategoryList = [.. GetRecipeCategoryList()],
            RecipeList = Get().ToList(),
        };
        return recipeVM;
    }

    /// <summary>
    /// Gets the site menu.
    /// </summary>
    /// <param name="domainId">The domain identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    public List<Models.MenuModel> GetSiteMenu(int domainId)
    {
        return CreateDomainMenu([.. webDomainContext.Recipe], domainId);
    }


    ///// <summary>
    ///// Sync Up Database with List of Categories
    ///// </summary>
    ///// <param name="saveItem">The save item.</param>
    ///// <returns>RecipeCategoryModel.</returns>
    public List<Models.RecipeCategoryModel> Save(List<Models.RecipeCategoryModel> saveCategories)
    {

        if (saveCategories == null) return [];

        var currentCategories = GetRecipeCategoryList();

        foreach (var saveItem in saveCategories)
        {
            if (saveItem == null) continue;

            if (string.IsNullOrWhiteSpace(saveItem.Name)) continue;

            var curCat = currentCategories.Where(w => w.Name == saveItem.Name).FirstOrDefault();

            saveItem.Id = curCat == null ? 0 : curCat.Id;

            if (saveItem.Id == 0)
            {
                var saveCategory = Create(saveItem);
                saveCategory.UpdatedDate = DateTime.UtcNow;
                webDomainContext.RecipeCategory.Add(saveCategory);
                webDomainContext.SaveChanges();
                saveItem.Id = saveCategory.Id;
            }
            else
            {
                var saveCategory = webDomainContext.RecipeCategory.Where(w => w.Id == saveItem.Id).FirstOrDefault();
                if (saveCategory != null)
                {
                    saveCategory.Name = saveItem.Name;
                    saveCategory.Comment = saveItem.Description;
                    saveCategory.UpdatedDate = DateTime.UtcNow;
                    webDomainContext.SaveChanges();
                }
            }
        }
        return GetRecipeCategoryList();
    }


    ///// <summary>
    ///// Sync Up Database with List of Categories
    ///// </summary>
    ///// <param name="saveItem">The save item.</param>
    ///// <returns>RecipeCategoryModel.</returns>
    public IEnumerable<Models.RecipeModel> Save(List<Models.RecipeModel> saveRecipes)
    {

        if (saveRecipes == null) return new List<Models.RecipeModel>();

        var curRecipes = Get();
        var currentCategories = GetRecipeCategoryList();

        foreach (var saveItem in saveRecipes)
        {
            if (saveItem == null) continue;

            if (string.IsNullOrWhiteSpace(saveItem.RecipeCategoryNM)) continue;

            var curRecipe = curRecipes.Where(w => w.Name == saveItem.Name).FirstOrDefault();

            saveItem.Id = curRecipe == null ? 0 : curRecipe.Id;
            var curCategory = currentCategories.Where(w => w.Name == saveItem.RecipeCategoryNM).FirstOrDefault();

            saveItem.RecipeCategoryID = curCategory == null ? 0 : curCategory.Id;

            if (saveItem.Id == 0)
            {
                var saveRecipe = Create(saveItem);
                saveRecipe.UpdatedDate = DateTime.UtcNow;
                webDomainContext.Recipe.Add(saveRecipe);
                webDomainContext.SaveChanges();
                saveItem.Id = saveRecipe.Id;
            }
            else
            {
                var saveRecipe = webDomainContext.Recipe.Where(w => w.Id == saveItem.Id).FirstOrDefault();
                if (saveRecipe != null)
                {
                    webDomainContext.SaveChanges();
                }
            }
        }
        return Get();
    }



    /// <summary>
    /// Saves the specified save item.
    /// </summary>
    /// <param name="saveItem">The save item.</param>
    /// <returns>RecipeModel.</returns>
    public Models.RecipeModel Save(Models.RecipeModel saveItem)
    {
        if (saveItem == null) return new Models.RecipeModel();
        if (saveItem.Id == 0)
        {
            var saveRecipe = Create(saveItem);
            webDomainContext.Recipe.Add(saveRecipe);
            webDomainContext.SaveChanges();
            saveItem.Id = saveRecipe.Id;
        }
        else
        {
            var saveRecipe = webDomainContext.Recipe.Where(w => w.Id == saveItem.Id).Include(i => i.RecipeCategory).FirstOrDefault();
            if (saveRecipe != null)
            {
                if (saveRecipe.RecipeCategory.Id != saveItem.RecipeCategoryID)
                {
                    saveRecipe.RecipeCategory = webDomainContext.RecipeCategory.Where(w => w.Id == saveItem.RecipeCategoryID).FirstOrDefault();
                }
                saveRecipe.Name = saveItem.Name;
                saveRecipe.AuthorName = saveItem.AuthorNM;
                saveRecipe.Name = saveItem.Name;
                saveRecipe.Description = saveItem.Description;
                saveRecipe.Ingredients = saveItem.Ingredients;
                saveRecipe.Instructions = saveItem.Instructions;
                saveRecipe.Servings = saveItem.Servings;
                saveRecipe.IsApproved = saveItem.IsApproved;

                webDomainContext.SaveChanges();
            }
        }
        return Get(saveItem.Id);
    }

    public Models.RecipeCategoryModel Save(Models.RecipeCategoryModel saveItem)
    {
        if (saveItem == null) return new Models.RecipeCategoryModel();
        if (saveItem.Id == 0)
        {
            var saveCategory = Create(saveItem);
            webDomainContext.RecipeCategory.Add(saveCategory);
            webDomainContext.SaveChanges();
            saveItem.Id = saveCategory.Id;
        }
        else
        {
            var saveCategory = webDomainContext.RecipeCategory.Where(w => w.Id == saveItem.Id).FirstOrDefault();
            if (saveCategory != null)
            {
                saveCategory.Name = saveItem.Name;
                saveCategory.Comment = saveItem.Description;
                saveCategory.DisplayOrder = saveItem.DisplayOrder;
                saveCategory.IsActive = saveItem.IsActive;


                webDomainContext.SaveChanges();
            }
        }
        return GetRecipeCategoryById(saveItem.Id);
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
            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~RecipeProvider()
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
