using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebSpark.Bootswatch;
using WebSpark.Core.Data;
using WebSpark.Core.Extensions;
using WebSpark.Core.Infrastructure.Logging;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Providers;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.Web.Extensions;
using Westwind.AspNetCore.Markdown;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
    .Build();

LoggingUtility.ConfigureLogging(builder, "WebSpark");

var AppDbConnectionString = builder.Configuration.GetValue("WebSparkContext", "Data Source=c:\\websites\\WebSpark\\webspark.db");
builder.Services.AddDbContext<WebSparkDbContext>(options =>
{
    options.UseSqlite(AppDbConnectionString);
});
builder.Services.AddSingleton<IScopeInformation, ScopeInformation>();
builder.Services.AddScoped<IWebsiteService, WebsiteProvider>();
builder.Services.AddScoped<IRecipeService, RecipeProvider>();
builder.Services.AddSingleton<WebRouteValueTransformer>();
builder.Services.AddSingleton<IWebsiteServiceFactory, WebsiteServiceFactory>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();

// Use our custom implementation instead of the default one for HTTP requests
builder.Services.AddScoped<IHttpRequestResultService, HttpRequestResultService>();
// Use the extension method to register Bootswatch theme switcher (includes StyleCache)
builder.Services.AddBootswatchThemeSwitcher();
builder.Services.AddLogging();

builder.Services.AddCors(o => o.AddPolicy("WebSparkPolicy", builder =>
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

builder.Services.AddMvc()
    .AddApplicationPart(typeof(MarkdownPageProcessorMiddleware).Assembly);

var app = builder.Build();
app.UseSession();
app.UseSessionInitialization();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("WebSparkPolicy");
app.MapRazorPages();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Page}/{action=Index}/{id?}");
app.MapDynamicControllerRoute<WebRouteValueTransformer>("{**slug}");
app.Run();
