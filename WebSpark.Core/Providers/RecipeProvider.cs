using WebSpark.Core.Data;
using WebSpark.Core.Infrastructure;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;
using WebSpark.Core.Models.ViewModels;

namespace WebSpark.Core.Providers;

/// <summary>
/// Recipe Service
/// Implements the <see cref="WebSparkDbContext" />
/// Implements the <see cref="IMenuProvider" />
/// </summary>
/// <seealso cref="WebSparkDbContext" />
/// <seealso cref="IMenuProvider" />
public class RecipeProvider(WebSparkDbContext webDomainContext) : IMenuProvider, IRecipeService, IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private List<RecipeModel> Create(List<Recipe>? list)
    {
        if (list == null) return [];
        return [.. list.Select(Create).OrderBy(x => x.Name)];
    }

    /// <summary>
    /// Creates the specified list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>List&lt;RecipeCategoryModel&gt;.</returns>
    private List<RecipeCategoryModel> Create(List<RecipeCategory>? list)
    {
        if (list == null) return [];
        return [.. list.Select(item => Create(item)).OrderBy(x => x.Name)];
    }

    /// <summary>
    /// Creates the specified recipe.
    /// </summary>
    /// <param name="Recipe">The recipe.</param>
    /// <returns>RecipeModel.</returns>
    private RecipeModel Create(Recipe? Recipe)
    {
        if (Recipe == null) return new RecipeModel();
        var r = Recipe; // local non-null alias

        return new RecipeModel()
        {
            DomainID = r.Domain?.Id ?? RecipeConstants.INT_MOM_DomainId,
            RecipeURL = FormatHelper.GetRecipeURL(r.Name),
            Id = r.Id,
            Name = r.Name,
            Ingredients = r.Ingredients,
            Instructions = r.Instructions,
            Description = string.IsNullOrEmpty(r.Description) ? r.Name : r.Description,
            SEO_Keywords = r.Keywords,
            Servings = r.Servings,
            AuthorNM = r.AuthorName,
            AverageRating = r.AverageRating,
            IsApproved = r.IsApproved,
            CommentCount = 0,
            RecipeCategory = Create(r.RecipeCategory),
            RecipeCategoryID = r.RecipeCategory?.Id ?? 0,
            RatingCount = r.RatingCount,
            ViewCount = r.ViewCount,
            LastViewDT = r.LastViewDt,
            ModifiedDT = r.UpdatedDate,
        };
    }

    /// <summary>
    /// Creates the specified recipe.
    /// </summary>
    /// <param name="Recipe">The recipe.</param>
    /// <returns>Recipe.</returns>
    private Recipe Create(RecipeModel? Recipe)
    {
        if (Recipe == null) return new Recipe();

        if (Recipe.DomainID == 0)
        {
            Recipe.DomainID = RecipeConstants.INT_MOM_DomainId;
        }

        var Category = webDomainContext.RecipeCategory.FirstOrDefault(w => w.Id == Recipe.RecipeCategoryID);
        var Domain = webDomainContext.Domain.FirstOrDefault(w => w.Id == Recipe.DomainID);

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
            RecipeCategory = Category ?? new RecipeCategory { Name = Recipe.Name, Comment = Recipe.Name },
            Domain = Domain ?? new WebSite { Name = Recipe.Name, Title = Recipe.Name },
            Servings = Recipe.Servings,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
        };
    }
    private static RecipeCategory Create(RecipeCategoryModel? s)
    {
        if (s == null) return new RecipeCategory();
        return new RecipeCategory()
        {
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            Comment = s.Name,
            Id = s.Id,
            Name = s.Name
        };
    }

    private List<RecipeImageModel> Create(List<RecipeImage>? dbRecipeImage)
    {
        var recipeImageList = new List<RecipeImageModel>();
        if (dbRecipeImage == null) return recipeImageList;
        foreach (var dbItem in dbRecipeImage)
        {
            if (dbItem == null) continue;
            recipeImageList.Add(Create(dbItem));
        }
        return recipeImageList;
    }

    private RecipeImageModel Create(RecipeImage? dbItem)
    {
        if (dbItem == null) return new RecipeImageModel();
        return new RecipeImageModel()
        {
            Id = dbItem.Id,
            Recipe = Create(dbItem.Recipe),
            DisplayOrder = dbItem.DisplayOrder,
            FileDescription = dbItem.FileDescription,
            FileName = dbItem.FileName,
        };
    }

    /// <summary>
    /// Creates the specified rc.
    /// </summary>
    /// <param name="rc">The rc.</param>
    /// <returns>RecipeCategoryModel.</returns>
    private RecipeCategoryModel Create(RecipeCategory? rc, bool LoadRecipes = false)
    {
        if (rc == null) return new RecipeCategoryModel();

        return new RecipeCategoryModel()
        {
            DomainID = rc.Domain?.Id ?? RecipeConstants.INT_MOM_DomainId,
            DisplayOrder = rc.DisplayOrder,
            IsActive = rc.IsActive,
            Description = rc.Comment,
            Id = rc.Id,
            Name = rc.Name,
            Url = FormatHelper.GetRecipeCategoryURL(rc.Name),
            Recipes = LoadRecipes ? Create(rc.Recipe?.ToList()) : []
        };
    }
    /// <summary>
    /// Creates the domain menu.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="DomainID">The domain identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    private List<MenuModel> CreateDomainMenu(List<Recipe>? list, int DomainID)
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
    private static List<MenuModel> CreateMenu(List<Recipe>? list)
    {
        if (list == null) return [];
        return [.. list.Select(item => GetMenuItem(item)).OrderBy(x => x.DisplayOrder)];
    }


    /// <summary>
    /// Gets the menu item.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <returns>MenuModel.</returns>
    private static MenuModel GetMenuItem(Recipe? recipe)
    {
        if (recipe == null) return new MenuModel();

        return new MenuModel()
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
    private static MenuModel GetMenuItem(Recipe? recipe, WebSite? domain)
    {
        if (recipe == null) return new MenuModel();

        return new MenuModel()
        {
            Id = recipe.Id,
            ParentId = recipe.RecipeCategory?.Id,
            Controller = "recipe",
            Action = "index",
            Description = recipe.Description,
            Title = recipe.Name,
            DomainID = domain?.Id ?? 0,
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
    private static MenuModel GetMenuItem(Recipe? recipe, WebSite? domain, Menu? page)
    {
        if (recipe == null) return new MenuModel();

        return new MenuModel()
        {
            Id = recipe.Id,
            ParentId = page?.Id,
            Controller = "recipe",
            ParentController = "recipe",
            ParentTitle = "recipe",
            Action = "index",
            Description = recipe.Description,
            Title = recipe.Name,
            Url = FormatHelper.GetRecipeURL(recipe.Name),
            DomainID = domain?.Id ?? 0,
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
    public bool Delete(RecipeCategoryModel saveItem)
    {
        var deleteItem = webDomainContext.RecipeCategory.Where(w => w.Id == saveItem.Id).FirstOrDefault();
        if (deleteItem != null)
        {
            webDomainContext.RecipeCategory.Remove(deleteItem);
            webDomainContext.SaveChanges();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the recipe list.
    /// </summary>
    /// <returns>List&lt;RecipeModel&gt;.</returns>
    public IEnumerable<RecipeModel> Get()
    {
        var theList = webDomainContext.Recipe.Include(r => r.RecipeCategory).Include(i => i.RecipeImage).ToList();
        return Create(theList);
    }

    /// <summary>
    /// Gets the by identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>RecipeModel.</returns>
    public RecipeModel Get(int Id)
    {
        var returnRecipe = Create(webDomainContext.Recipe.Where(w => w.Id == Id).Include(r => r.RecipeCategory).FirstOrDefault());
        returnRecipe.RecipeCategories = webDomainContext.RecipeCategory.Select(s => new LookupModel() { Value = s.Id.ToString(), Text = s.Name }).ToList();
        return returnRecipe;
    }


    /// <summary>
    /// Gets the menu list.
    /// </summary>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    public List<MenuModel> GetAllMenuItems()
    {
        return CreateMenu([.. webDomainContext.Recipe]);
    }

    /// <summary>
    /// Get Menu Item for a Recipe Id
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>MenuModel.</returns>
    public MenuModel GetMenuItem(int Id)
    {
        return GetMenuItem(webDomainContext.Recipe.Where(w => w.Id == Id).FirstOrDefault());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public async Task<MenuModel> GetMenuItemAsync(int Id)
    {
        var returnMenu = GetMenuItem(await webDomainContext.Set<Recipe>().Where(w => w.Id == Id).FirstOrDefaultAsync());
        if (returnMenu == null)
            returnMenu = new MenuModel();
        return returnMenu;
    }

    public IEnumerable<MenuModel> GetMenuList()
    {
        return new List<MenuModel>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public RecipeCategoryModel GetRecipeCategoryById(int Id)
    {
        var returnCategory = new RecipeCategoryModel();
        returnCategory = Create(webDomainContext.RecipeCategory.Include(i => i.Recipe).Where(w => w.Id == Id).FirstOrDefault(), LoadRecipes: true);
        return returnCategory;
    }

    /// <summary>
    /// Gets the recipe category list.
    /// </summary>
    /// <returns>List&lt;RecipeCategoryModel&gt;.</returns>
    public List<RecipeCategoryModel> GetRecipeCategoryList()
    {
        return Create(webDomainContext.RecipeCategory.ToList());
    }

    public List<RecipeImageModel> GetRecipeImages()
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
    public List<MenuModel> GetSiteMenu(int domainId)
    {
        return CreateDomainMenu([.. webDomainContext.Recipe], domainId);
    }


    ///// <summary>
    ///// Sync Up Database with List of Categories
    ///// </summary>
    ///// <param name="saveItem">The save item.</param>
    ///// <returns>RecipeCategoryModel.</returns>
    public List<RecipeCategoryModel> Save(List<RecipeCategoryModel>? saveCategories)
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
    public IEnumerable<RecipeModel> Save(List<RecipeModel>? saveRecipes)
    {

        if (saveRecipes == null) return new List<RecipeModel>();

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
    public RecipeModel Save(RecipeModel? saveItem)
    {
        if (saveItem == null) return new RecipeModel();
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
                if (saveRecipe.RecipeCategory == null || saveRecipe.RecipeCategory.Id != saveItem.RecipeCategoryID)
                {
                    saveRecipe.RecipeCategory = (webDomainContext.RecipeCategory.FirstOrDefault(w => w.Id == saveItem.RecipeCategoryID) ?? saveRecipe.RecipeCategory)!;
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

    public RecipeCategoryModel Save(RecipeCategoryModel saveItem)
    {
        if (saveItem == null) return new RecipeCategoryModel();
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
                webDomainContext?.Dispose();
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
