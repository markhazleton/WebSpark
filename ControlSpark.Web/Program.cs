using ControlSpark.Core.Data;
using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.Web.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;
using Westwind.AspNetCore.Markdown;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File(@"C:\temp\controlspark-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Logging.AddProvider(new SerilogLoggerProvider(Log.Logger));
Log.Information("Logger setup complete. This is a test log entry.");

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
    .Build();


// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IScopeInformation, ScopeInformation>();
builder.Services.AddScoped<IWebsiteService, WebsiteProvider>();
builder.Services.AddScoped<IRecipeService, RecipeProvider>();
builder.Services.AddSingleton<WebRouteValueTransformer>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();

builder.Services.AddCors(o => o.AddPolicy("ControlSparkPolicy", builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddBlogDatabase(config);
builder.Services.AddBlogProviders();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(360);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// We need to use MVC so we can use a Razor Configuration SiteTemplate
// have to let MVC know we have a controller
builder.Services.AddMvc()
    .AddApplicationPart(typeof(MarkdownPageProcessorMiddleware).Assembly);

// Setup Database and Seed (TEMP)
var app = builder.Build();
//await DbInitializer.SeedAsync(app);

app.UseSession();
app.Use(async (context, next) =>
{
    if (context == null || context.Session == null || context.Session.GetInt32(SessionExtensionsKeys.SessionInitialized) == 1)
    {
        return;
    }
    context.Session.Set("MyTest", "MyTest");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("ControlSparkPolicy");

app.MapRazorPages();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Page}/{action=Index}/{id?}"); 
app.MapDynamicControllerRoute<WebRouteValueTransformer>("{**slug}");

app.Run();
