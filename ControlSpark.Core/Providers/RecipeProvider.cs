using ControlSpark.Core.Infrastructure;
using ControlSpark.RecipeManager.Interfaces;

namespace ControlSpark.Core.Providers;

/// <summary>
/// Recipe Service
/// Implements the <see cref="AppDbContext" />
/// Implements the <see cref="IMenuProvider" />
/// </summary>
/// <seealso cref="AppDbContext" />
/// <seealso cref="IMenuProvider" />
public class RecipeProvider : IMenuProvider, IRecipeService, IDisposable
{
    private readonly AppDbContext _context;

    public RecipeProvider(AppDbContext webDomainContext)
    {
        _context = webDomainContext;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private List<RecipeModel> Create(List<Recipe> list)
    {
        if (list == null) return new List<RecipeModel>();
        return list.Select(item => Create(item)).OrderBy(x => x.Name).ToList();
    }

    /// <summary>
    /// Creates the specified list.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>List&lt;RecipeCategoryModel&gt;.</returns>
    private List<RecipeCategoryModel> Create(List<RecipeCategory> list)
    {
        if (list == null) return new List<RecipeCategoryModel>();
        return list.Select(item => Create(item)).OrderBy(x => x.Name).ToList();
    }

    /// <summary>
    /// Creates the specified recipe.
    /// </summary>
    /// <param name="Recipe">The recipe.</param>
    /// <returns>RecipeModel.</returns>
    private RecipeModel Create(Recipe Recipe)
    {
        if (Recipe == null) return new RecipeModel();

        return new RecipeModel()
        {
            RecipeURL = FormatHelper.GetRecipeURL(Recipe.Name),
            Id = Recipe.Id,
            Name = Recipe.Name,
            Ingredients = Recipe.Ingredients,
            Instructions = Recipe.Instructions,
            Description = Recipe.Description,
            AuthorNM = Recipe.AuthorName,
            AverageRating = Recipe.AverageRating,
            IsApproved = Recipe.IsApproved,
            CommentCount = 0,
            RecipeCategory = Create(Recipe.RecipeCategory),
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
    private Recipe Create(RecipeModel Recipe)
    {
        if (Recipe == null) return new Recipe();

        var Category = _context.RecipeCategory.Where(w => w.Id == Recipe.RecipeCategory.Id).FirstOrDefault();

        return new Recipe()
        {
            Id = Recipe.Id,
            Name = Recipe.Name,
            Ingredients = Recipe.Ingredients,
            Instructions = Recipe.Instructions,
            Description = string.IsNullOrEmpty(Recipe.Description) ? Recipe.Name : Recipe.Description,
            AuthorName = Recipe.AuthorNM,
            AverageRating = Recipe.AverageRating,
            IsApproved = Recipe.IsApproved,
            CommentCount = Recipe.CommentCount,
            RatingCount = Recipe.RatingCount,
            ViewCount = Recipe.ViewCount,
            LastViewDt = Recipe.LastViewDT,
            RecipeCategory = Category

        };
    }
    private RecipeCategory Create(RecipeCategoryModel s)
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

    /// <summary>
    /// Creates the specified rc.
    /// </summary>
    /// <param name="rc">The rc.</param>
    /// <returns>RecipeCategoryModel.</returns>
    private RecipeCategoryModel Create(RecipeCategory rc, bool LoadRecipes = false)
    {
        if (rc == null) return new RecipeCategoryModel();

        return new RecipeCategoryModel()
        {
            DisplayOrder = rc.DisplayOrder,
            IsActive = rc.IsActive,
            Description = rc.Comment,
            Id = rc.Id,
            Name = rc.Name,
            Url = FormatHelper.GetRecipeCategoryURL(rc.Name),
            Recipes = LoadRecipes ? Create(rc.Recipe.ToList()) : new List<RecipeModel>()
        };
    }
    /// <summary>
    /// Creates the domain menu.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="DomainID">The domain identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    private List<MenuModel> CreateDomainMenu(List<Recipe> list, int DomainID)
    {
        if (list == null) return new List<MenuModel>();

        if (DomainID > 0)
        {
            var webSite = _context.Domain.Where(w => w.Id == DomainID).FirstOrDefault();
            var page = _context.Menu.Where(w => w.Id == DomainID).Where(w => w.Title == "Recipe").FirstOrDefault();

            if (webSite != null)
            {
                if (page == null)
                {
                    return list.Select(item => GetMenuItem(item, webSite)).OrderBy(x => x.DisplayOrder).ToList();
                }
                else
                {
                    return list.Select(item => GetMenuItem(item, webSite, page)).OrderBy(x => x.DisplayOrder).ToList();
                }
            }
            else
            {
                return list.Select(item => GetMenuItem(item)).OrderBy(x => x.DisplayOrder).ToList();
            }
        }
        return new List<MenuModel>();

    }


    /// <summary>
    /// Creates the menu.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    private List<MenuModel> CreateMenu(List<Recipe> list)
    {
        if (list == null) return new List<MenuModel>();
        return list.Select(item => GetMenuItem(item)).OrderBy(x => x.DisplayOrder).ToList();
    }


    /// <summary>
    /// Gets the menu item.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <returns>MenuModel.</returns>
    private MenuModel GetMenuItem(Recipe recipe)
    {
        if (recipe == null) return new MenuModel();

        return new MenuModel()
        {
            Id = recipe.Id,
            ParentId = recipe.RecipeCategory?.Id,
            Controller = "Recipe",
            Action = "index",
            Description = recipe.Name,
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
    private MenuModel GetMenuItem(Recipe recipe, WebSite domain)
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
    private MenuModel GetMenuItem(Recipe recipe, WebSite domain, Menu page)
    {
        if (recipe == null) return new MenuModel();

        return new MenuModel()
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
        var deleteItem = _context.Recipe.Where(w => w.Id == Id).FirstOrDefault();
        if (deleteItem != null)
        {
            _context.Recipe.Remove(deleteItem);
            _context.SaveChanges();
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        ((IDisposable)_context).Dispose();
    }

    /// <summary>
    /// Gets the recipe list.
    /// </summary>
    /// <returns>List&lt;RecipeModel&gt;.</returns>
    public IEnumerable<RecipeModel> Get()
    {
        return Create(_context.Recipe.Include(r => r.RecipeCategory).ToList());
    }

    /// <summary>
    /// Gets the by identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>RecipeModel.</returns>
    public RecipeModel Get(int Id)
    {
        var returnRecipe = new RecipeModel();
        returnRecipe = Create(_context.Recipe.Where(w => w.Id == Id).Include(r => r.RecipeCategory).FirstOrDefault());
        return returnRecipe;
    }


    /// <summary>
    /// Gets the menu list.
    /// </summary>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    public List<MenuModel> GetAllMenuItems()
    {
        return CreateMenu(_context.Recipe.ToList());
    }

    /// <summary>
    /// Get Menu Item for a Recipe Id
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>MenuModel.</returns>
    public MenuModel GetMenuItem(int Id)
    {
        return GetMenuItem(_context.Recipe.Where(w => w.Id == Id).FirstOrDefault());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public async Task<MenuModel> GetMenuItemAsync(int Id)
    {
        var returnMenu = GetMenuItem(await _context.Set<Recipe>().Where(w => w.Id == Id).FirstOrDefaultAsync());
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
        returnCategory = Create(_context.RecipeCategory.Include(i => i.Recipe).Where(w => w.Id == Id).FirstOrDefault(), LoadRecipes: true);
        return returnCategory;
    }

    /// <summary>
    /// Gets the recipe category list.
    /// </summary>
    /// <returns>List&lt;RecipeCategoryModel&gt;.</returns>
    public List<RecipeCategoryModel> GetRecipeCategoryList()
    {
        return Create(_context.RecipeCategory.ToList());
    }

    /// <summary>
    /// Gets the site menu.
    /// </summary>
    /// <param name="domainId">The domain identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    public List<MenuModel> GetSiteMenu(int domainId)
    {
        return CreateDomainMenu(_context.Recipe.ToList(), domainId);
    }


    ///// <summary>
    ///// Sync Up Database with List of Categories
    ///// </summary>
    ///// <param name="saveItem">The save item.</param>
    ///// <returns>RecipeCategoryModel.</returns>
    public List<RecipeCategoryModel> Save(List<RecipeCategoryModel> saveCategories)
    {

        if (saveCategories == null) return new List<RecipeCategoryModel>();

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
                saveCategory.UpdatedDate = DateTime.Now;
                _context.RecipeCategory.Add(saveCategory);
                _context.SaveChanges();
                saveItem.Id = saveCategory.Id;
            }
            else
            {
                var saveCategory = _context.RecipeCategory.Where(w => w.Id == saveItem.Id).FirstOrDefault();
                if (saveCategory != null)
                {
                    saveCategory.Name = saveItem.Name;
                    saveCategory.Comment = saveItem.Description;
                    saveCategory.UpdatedDate = DateTime.Now;
                    _context.SaveChanges();
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
    public IEnumerable<RecipeModel> Save(List<RecipeModel> saveRecipes)
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
                saveRecipe.UpdatedDate = DateTime.Now;
                _context.Recipe.Add(saveRecipe);
                _context.SaveChanges();
                saveItem.Id = saveRecipe.Id;
            }
            else
            {
                var saveRecipe = _context.Recipe.Where(w => w.Id == saveItem.Id).FirstOrDefault();
                if (saveRecipe != null)
                {
                    _context.SaveChanges();
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
    public RecipeModel Save(RecipeModel saveItem)
    {
        if (saveItem == null) return new RecipeModel();
        if (saveItem.Id == 0)
        {
            var saveRecipe = Create(saveItem);
            _context.Recipe.Add(saveRecipe);
            _context.SaveChanges();
            saveItem.Id = saveRecipe.Id;
        }
        else
        {
            var saveRecipe = _context.Recipe.Where(w => w.Id == saveItem.Id).Include(i => i.RecipeCategory).FirstOrDefault();
            if (saveRecipe != null)
            {
                if (saveRecipe.RecipeCategory.Id != saveItem.RecipeCategoryID)
                {
                    saveRecipe.RecipeCategory = _context.RecipeCategory.Where(w => w.Id == saveItem.RecipeCategoryID).FirstOrDefault();
                }
                saveRecipe.Name = saveItem.Name;
                saveRecipe.AuthorName = saveItem.AuthorNM;
                saveRecipe.Name = saveItem.Name;
                saveRecipe.Description = saveItem.Description;
                saveRecipe.Ingredients = saveItem.Ingredients;
                saveRecipe.Instructions = saveItem.Instructions;

                _context.SaveChanges();
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
            _context.RecipeCategory.Add(saveCategory);
            _context.SaveChanges();
            saveItem.Id = saveCategory.Id;
        }
        else
        {
            var saveCategory = _context.RecipeCategory.Where(w => w.Id == saveItem.Id).FirstOrDefault();
            if (saveCategory != null)
            {
                saveCategory.Name = saveItem.Name;
                saveCategory.Comment = saveItem.Description;
                saveCategory.DisplayOrder = saveItem.DisplayOrder;
                saveCategory.IsActive = saveItem.IsActive;


                _context.SaveChanges();
            }
        }
        return GetRecipeCategoryById(saveItem.Id);
    }

    public RecipeVM GetRecipeVMHostAsync(string host, WebsiteVM baseVM)
    {
        var recipeVM = new RecipeVM(baseVM)
        {
            CategoryList = GetRecipeCategoryList().ToList(),
            RecipeList = Get().ToList(),
        };
        return recipeVM;
    }
}
