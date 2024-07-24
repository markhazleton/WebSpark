using System.IO;
using System.Text.Json;
using WebSpark.Core.Helpers;
using WebSpark.Core.Infrastructure.BaseClasses;
using WebSpark.Domain.Models;

namespace WebSpark.Core.Data;

public class SeedDatabase : IDisposable
{
    private readonly WebSparkDbContext _context;

    public SeedDatabase(WebSparkDbContext context)
    {
        _context = context;
    }

    private static WebSite GetMomWebsite()
    {
        var Mom = new WebSite()
        {
            Name = "MechanicsOfMotherhood.com",
            Description = "MechanicsOfMotherhood.com",
            Title = "MechanicsOfMotherhood.com",
            DomainUrl = "mom.controlorigins.com",
            Template = "ControlSpark",
            Style = "mom",
            VersionNo = 1,
            UseBreadCrumbUrl = true,
            GalleryFolder = "mom"
        };

        Mom.Menus.Add(WebsiteHelper.GetMenuPage("Mom", "# Welcome to the Mechanics Of Motherhood \n ## The Mechanics of being a Mom \n\n\n\n\n"));

        var momPage = Mom.Menus.Where(w => w.Title == "Mom").FirstOrDefault();
        Mom.Menus.Add(WebsiteHelper.GetMenuPage(
            "About",
            "# About \n About Mechanics of Motherhood",
            FontAwesomeIcons.chevron,
            momPage));
        Mom.Menus.Add(WebsiteHelper.GetMenuPage(
            "Bootswatch",
            null,
            FontAwesomeIcons.cog,
            momPage,
            "Bootswatch"));

        Mom.Menus.Add(WebsiteHelper.GetMenuPage(
            "Recipe",
            null,
            FontAwesomeIcons.bolt,
            null,
            "Recipe"));

        Mom.Menus.Add(WebsiteHelper.GetMenuPage(
            "Thoughts",
            "# Thoughts \n Thoughts from the in house Mom and Dad",
            FontAwesomeIcons.comment,
            parent: null,
            controllerName: "Blog",
            actionName: "index",
            argumentName: string.Empty));
        var thoughtsPage = Mom.Menus.Where(w => w.Title == "Thoughts").FirstOrDefault();
        Mom.Menus.Add(WebsiteHelper.GetMenuPage(
            "Mother",
            "# Mother \n > Thoughts from the in house mother ",
            FontAwesomeIcons.heart,
            thoughtsPage,
            controllerName: "Blog",
            actionName: "categories",
            argumentName: "Mom Thoughts"));
        Mom.Menus.Add(WebsiteHelper.GetMenuPage(
            "Dad",
            "# Dad \n > Thoughts from the in house father ",
            FontAwesomeIcons.coffee,
            thoughtsPage,
            controllerName: "Blog",
            actionName: "categories",
            argumentName: "Dad Thoughts"));


        return Mom;
    }
    private static WebSite GetProjectMechanicsWebsite()
    {
        var projectMechanics = new WebSite()
        {
            Name = "ProjectMechanics",
            Description = "The mechanics of getting IT done",
            Title = "ProjectMechanics",
            DomainUrl = "pm.controlorigins.com",
            Template = "ControlSpark",
            Style = "Slate",
            VersionNo = 1,
            UseBreadCrumbUrl = true,
            GalleryFolder = "pm"
        };

        projectMechanics.Menus.Add(WebsiteHelper.GetMenuPage("ProjectMechanics",
                "# Welcome to **Project Mechanics**\n```    \n this is a code example  \n```\n## Have a Great Day!"));
        var parentPage = projectMechanics.Menus.Where(w => w.Title == "TexEcon").FirstOrDefault();
        projectMechanics.Menus.Add(WebsiteHelper.GetMenuPage("Mark Hazleton", "# Mark Hazleton \n ## ", FontAwesomeIcons.chevron, parentPage));
        projectMechanics.Menus.Add(WebsiteHelper.GetMenuPage("Bootswatch", null, FontAwesomeIcons.cog, parentPage, "Bootswatch"));
        var i = 1;
        foreach (var page in projectMechanics.Menus)
        {
            page.DisplayOrder = (i * 10);
            i++;
        }
        return projectMechanics;
    }
    private static WebSite GetTexeconWebsite()
    {
        var Texecon = new WebSite()
        {
            Name = "TexEcon.com",
            Description = "TexEcon.com",
            Title = "TexEcon.com",
            DomainUrl = "texecon.controlorigins.com",
            Template = "ControlSpark",
            Style = "Zephyr",
            VersionNo = 1,
            UseBreadCrumbUrl = true,
            GalleryFolder = "texecon"
        };

        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("TexEcon",
            "# Welcome to **TexEcon.com**\n```    \n this is a code example  \n```\n## Have a Great Day!"));
        var parentPage = Texecon.Menus.Where(w => w.Title == "TexEcon").FirstOrDefault();
        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Mark Hazleton", "# Mark Hazleton \n ## ", FontAwesomeIcons.chevron, parentPage));
        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Jared Hazleton", "# Jared Hazleton \n ## Economist, Educator, [Author](https://www.goodreads.com/author/show/1405162.Jared_E_Hazleton) \n \n > Jared Earl Hazleton was born on September 12, 1937 in Oklahoma City, Oklahoma, United States to Alfred Larson and Myrtle Frances Hazleton.  \n ### Education \n - Bachelor of Business Administration, University Oklahoma, Norman, 1959. \n - Doctor of Philosophy in Economics, Rice University, Houston, 1961. \n \n ### Career \n - Recipient, John W. Gardner Award, Rice University, 1965", FontAwesomeIcons.chevron, parentPage));
        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Bootswatch", null, FontAwesomeIcons.cog, parentPage, "Bootswatch"));

        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Texas", "# Texas  \n > The Lone Star State  \n > The Friendly State  \n \n <br/>   ![Texas](https://upload.wikimedia.org/wikipedia/commons/a/ad/Texas_in_United_States.svg)  ", FontAwesomeIcons.heart));
        parentPage = Texecon.Menus.Where(w => w.Title == "Texas").FirstOrDefault();
        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Austin", "# Austin \n > Austin is a great place to visit or live \n", FontAwesomeIcons.heart, parentPage));
        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Dallas", "# Dallas \n > Big D the heart of the metroplex", FontAwesomeIcons.heart, parentPage));
        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Houston", "# Houston \n > Texas Home for Diversity", FontAwesomeIcons.heart, parentPage));
        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Keller", "# Keller \n >  **We Love Keller** \n \n <iframe width='560' height='315' src='https://www.youtube.com/embed/db45f7ESwxc' frameborder='0' allowfullscreen=''></iframe>", FontAwesomeIcons.heart, parentPage));
        Texecon.Menus.Add(WebsiteHelper.GetMenuPage("Roanoke", "# Roanoke, Texas \n  > Small Town Charm & Big City Heart  \n \n Roanoke offers a unique quality of life and has friendly small town charm and the amenities of a big city in the heart of the Metroplex.", FontAwesomeIcons.heart, parentPage));

        var i = 1;
        foreach (var page in Texecon.Menus)
        {
            page.DisplayOrder = (i * 10);
            i++;
        }
        return Texecon;
    }

    public void Dispose()
    {
        ((IDisposable)_context).Dispose();
    }

    public async Task SeedDatabaseAsync()
    {
        try
        {
            _context.Domain.Add(GetTexeconWebsite());
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Texecon Load Exception");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException?.Message);
        }
        try
        {
            var dOrder = 1;
            var Mom = GetMomWebsite();
            _context.Domain.Add(Mom);
            _context.SaveChanges();

            var catlist = new List<string>() { "Appetizer", "Bread", "Breakfast", "Dessert", "Dips", "Drink", "Main Course", "Quick Meals", "Salad", "Sauce", "Side Dishes", "Slow Cooker", "Soup", "Vegetable" };
            foreach (var Name in catlist)
            {
                _context.RecipeCategory.Add(RecipeHelper.GetRecipeCategory(Name, dOrder));
                dOrder++;
            }
            await _context.SaveChangesAsync();

            string jsonFilePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"wwwroot\site\mom\RecipeList.json");


            // var jsonFilePath = @"~\site\mom\RecipeList.json";
            var json = File.ReadAllText(jsonFilePath);
            var recipeOlds = JsonSerializer.Deserialize<List<RecipeOld>>(json);
            RecipeCategory cat;
            foreach (var item in recipeOlds)
            {
                cat = _context.RecipeCategory.Where(w => w.Id == item.RecipeCategoryID).FirstOrDefault();
                _context.Recipe.Add(item.GetRecipe(Mom, cat));
                _context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("MOM Load Exception");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException?.Message);
        }
        try
        {
            _context.AddRange(GetProjectMechanicsWebsite());
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine("ProjectMechanics Load Exception");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException?.Message);
        }

        var myBlog = new Blog();

        try
        {

            var markAuthor = new Author()
            {
                Id = 1,
                Email = "mark@frogsfolly.com",
                Password = "Kn+NlXX9cqOYrb1WMptB5pf51Vb/FuumI1kHbxXSivA=",
                DisplayName = "Mark Hazleton",
                Bio = "Solutions Architect, lifelong learner, passionate for solutions which make technology work for business, not sidetracked by sizzle",
                Avatar = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 250 250' width='250' height='250'%3E%3Crect width='250' height='250' fill='%23622aff'%3E%3C/rect%3E%3Ctext x='50%' y='53%' dominant-baseline='middle' text-anchor='middle' font-family='Arial, sans-serif' font-size='128px' fill='%23ffffff'%3EM%3C/text%3E%3C/svg%3E",
                IsAdmin = true,
                CreatedDate = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedID = 1,
                UpdatedID = 1
            };
            _context.Authors.Add(markAuthor);
            _context.SaveChanges();

            myBlog = new Blog()
            {
                Id = 1,
                Title = "Thoughts",
                Description = "Thoughts from Mom and Dad",
                Theme = "mom",
                IncludeFeatured = true,
                ItemsPerPage = 10,
                AnalyticsListType = 2,
                AnalyticsPeriod = 5,
                CreatedDate = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                FooterScript = null,
                HeaderScript = null,
                Cover = "data/2/2022/5/DSC00279.JPG",
                Logo = null,
                CreatedID = 1,
                UpdatedID = 1,
                Authors = [markAuthor],
            };

            _context.Blogs.Add(myBlog);
            _context.SaveChanges();

            _context.Categories.Add(GetBlogCategory(1, "MechanicsOfMotherhood"));
            _context.Categories.Add(GetBlogCategory(2, "Mom Thoughts"));
            _context.Categories.Add(GetBlogCategory(3, "Dad Thoughts"));
            _context.SaveChanges();


        }
        catch (Exception ex)
        {
            Console.WriteLine("ControlSpark Exception");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException?.Message);
        }
        try
        {
            var myBlogPost = new Post()
            {
                Id = 1,
                Title = "MechanicsOfMotherhood",
                Slug = "mechanics-of-motherhood",
                Description = "How to focus on business value rather than technical wow.",
                Content = "How to focus on business value rather than technical wow.",
                Cover = "data/2/2022/5/DSC00279.JPG",
                CreatedDate = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Published = DateTime.UtcNow,
                CreatedID = 1,
                UpdatedID = 1,
                Blog = myBlog,
                AuthorId = 1,
                Selected = true,
                IsFeatured = true,
                PostCategories = [GetPostCategory(1, 1)],
                PostType = PostType.Post,
                PostViews = 1,
                Rating = 5,
            };
            _context.Posts.Add(myBlogPost);
            _context.SaveChanges();

            myBlogPost = new Post()
            {
                Id = 2,
                Title = "FromMomPost",
                Slug = "from-mom-post",
                Description = "This is a post from MOM.",
                Content = "This is a post from MOM.",
                Cover = "data/2/2022/5/DSC00279.JPG",
                CreatedDate = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Published = DateTime.UtcNow,
                CreatedID = 1,
                UpdatedID = 1,
                Blog = myBlog,
                AuthorId = 1,
                Selected = true,
                IsFeatured = true,
                PostCategories = [GetPostCategory(2, 2)],
                PostType = PostType.Post,
                PostViews = 1,
                Rating = 5,
            };
            _context.Posts.Add(myBlogPost);
            _context.SaveChanges();

            myBlogPost = new Post()
            {
                Id = 3,
                Title = "FromDadPost",
                Slug = "from-dad-post",
                Description = "This is a post from DAD.",
                Content = "This is a post from DAD.",
                Cover = "data/2/2022/5/DSC00279.JPG",
                CreatedDate = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Published = DateTime.UtcNow,
                CreatedID = 1,
                UpdatedID = 1,
                Blog = myBlog,
                AuthorId = 1,
                Selected = true,
                IsFeatured = true,
                PostCategories = [GetPostCategory(3, 3)],
                PostType = PostType.Post,
                PostViews = 1,
                Rating = 5,
            };
            _context.Posts.Add(myBlogPost);
            _context.SaveChanges();

        }
        catch (Exception ex)
        {
            Console.WriteLine("ControlSpark Exception");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException?.Message);
        }



        _context.ChangeTracker.Clear();

        static Category GetBlogCategory(int id, string catName)
        {
            return new Category()
            {
                Id = id,
                Content = catName,
                Description = catName,
                CreatedDate = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
        }
        static PostCategory GetPostCategory(int postID, int categoryId)
        {
            return new PostCategory()
            {
                PostId = postID,
                CategoryId = categoryId
            };
        }
    }
}
