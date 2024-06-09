using HttpClientUtility;
using HttpClientUtility.StringConverter;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;
using WebSpark.Core.Data;
using WebSpark.Core.Extensions;
using WebSpark.Core.Providers;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Service;
using WebSpark.RecipeManager.Interfaces;
using WebSpark.WebMvc.Areas.Identity.Data;
using WebSpark.WebMvc.Service;
using Westwind.AspNetCore.Markdown;
using HttpClientUtility.FullService;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>();

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File(@"C:\temp\webspark\webspark-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Logging.AddProvider(new SerilogLoggerProvider(Log.Logger));
Log.Information("Logger setup complete. This is a test log entry.");

var connectionString = builder.Configuration.GetConnectionString("ControlSparkUserContextConnection")
    ?? throw new InvalidOperationException("Connection string 'ControlSparkUserContextConnection' not found.");

builder.Services.AddDbContext<ControlSparkUserContext>(options => options.UseSqlite(connectionString));
builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDefaultIdentity<ControlSparkUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ControlSparkUserContext>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});

builder.Services.AddDbContext<GPTDbContext>(options =>
{
    options.UseSqlite(builder.Configuration["ConnectionStrings:PromptSparkConnection"]);
});
builder.Services.AddSingleton<IStringConverter, NewtonsoftJsonStringConverter>();
builder.Services.AddHttpClient("HttpClientService", client =>
{
    client.Timeout = TimeSpan.FromMilliseconds(90000);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientService");
    client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
    client.DefaultRequestHeaders.Add("X-Request-Source", "HttpClientService");
});
builder.Services.AddScoped(serviceProvider =>
{
    IHttpClientService baseService = new HttpClientService(
        serviceProvider.GetRequiredService<IHttpClientFactory>(),
        serviceProvider.GetRequiredService<IStringConverter>()
        );

    IHttpClientService telemetryService = new HttpClientServiceTelemetry(
        baseService);

    return telemetryService;
});

builder.Services.AddScoped(serviceProvider =>
{
    IHttpClientFullService baseService = new HttpClientFullService(
        serviceProvider.GetRequiredService<IHttpClientFactory>(),
        serviceProvider.GetRequiredService<IStringConverter>()
        );

    IHttpClientFullService telemetryService = new HttpClientFullServiceTelemetry(
        baseService);

    return telemetryService;
});



builder.Services.AddScoped<IUserPromptService, UserPromptService>();
builder.Services.AddScoped<IGPTDefinitionService, GPTDefinitionService>();
builder.Services.AddScoped<IGPTDefinitionTypeService, GPTDefinitionTypeService>();
builder.Services.AddScoped<IGPTService, OpenAIChatCompletionService>();


// Add services to the container.
builder.Services.AddMarkdown();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IScopeInformation, ScopeInformation>();
builder.Services.AddScoped<IWebsiteService, WebsiteProvider>();
builder.Services.AddScoped<IMenuService, MenuProvider>();
builder.Services.AddScoped<IRecipeService, RecipeProvider>();
builder.Services.AddScoped<IMenuProvider, MenuProvider>();
builder.Services.AddScoped<IRecipeImageService, RecipeImageService>();
builder.Services.AddScoped<IRecipeGPTService, RecipePromptSparkService>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseMarkdown();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); ;

app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Main}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
