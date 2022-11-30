using ControlSpark.Core.Data;
using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.Web.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Westwind.AspNetCore.Markdown;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", false, true)
                            .AddEnvironmentVariables()
                            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                            .Build();

Log.Logger = new LoggerConfiguration()
      .Enrich.FromLogContext()
      .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
      .CreateLogger();

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IScopeInformation, ScopeInformation>();
builder.Services.AddScoped<IWebsiteService, WebsiteProvider>();
builder.Services.AddScoped<IMenuService, MenuProvider>();
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
// We need to use MVC so we can use a Razor Configuration Template
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
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("ControlSparkPolicy");

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Page}/{action=Index}/{id?}");
    endpoints.MapDynamicControllerRoute<WebRouteValueTransformer>("{**slug}");
});
app.Run();
