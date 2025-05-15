using HttpClientUtility.MemoryCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using OpenWeatherMapClient.Interfaces;
using OpenWeatherMapClient.WeatherService;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.PromptSparkChat;
using PromptSpark.Domain.Service;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TriviaSpark.Domain.Entities;
using TriviaSpark.Domain.OpenTriviaDb;
using TriviaSpark.Domain.Services;
using TriviaSpark.JShow.Data;
using TriviaSpark.JShow.Models;
using TriviaSpark.JShow.Service;
using WebSpark.Core.Data;
using WebSpark.Core.Infrastructure.Logging;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;
using WebSpark.Core.Providers;
using WebSpark.HttpClientUtility.Crawler;
using WebSpark.HttpClientUtility.MemoryCache;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;
using WebSpark.Portal.Areas.DataSpark.Services;
using WebSpark.Portal.Areas.GitHubSpark.Extensions;
using WebSpark.RecipeCookbook;
using Westwind.AspNetCore.Markdown;
using WebSpark.Bootswatch;

var builder = WebApplication.CreateBuilder(args);

// ========================
// Configuration
// ========================
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>();

// ========================
// Logging Configuration
// ========================
LoggingUtility.ConfigureLogging(builder, "WebSparkPortal");

// ========================
// Caching Services
// ========================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<IMemoryCacheManager, MemoryCacheManager>();

// ========================
// Session Configuration
// ========================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ========================
// Database Contexts
// ========================
// Admin User Context
var adminConnectionString = builder.Configuration.GetValue("WebSparkUserContext", "Data Source=c:\\websites\\WebSpark\\ControlSparkUser.db");
builder.Services.AddDbContext<WebSparkUserContext>(options =>
    options.UseSqlite(adminConnectionString));

// Main Application Context
var AppDbConnectionString = builder.Configuration.GetValue("WebSparkContext", "Data Source=c:\\websites\\WebSpark\\webspark.db");
builder.Services.AddDbContext<WebSparkDbContext>(options =>
{
    options.UseSqlite(AppDbConnectionString);
});

// GPT Context
var GPTDbConnectionString = builder.Configuration.GetValue("GPTDbContext", "Data Source=c:\\websites\\WebSpark\\PromptSpark.db");
builder.Services.AddDbContext<GPTDbContext>(options =>
    options.UseSqlite(GPTDbConnectionString));

// Trivia Spark Context
var TriviaDbConnectionString = builder.Configuration.GetValue("TriviaDbContext", "Data Source=c:\\websites\\WebSpark\\TriviaSpark.db");
builder.Services.AddDbContext<TriviaSparkDbContext>(options =>
    options.UseSqlite(TriviaDbConnectionString));

// JShow Context
var JShowDbConnectionString = builder.Configuration.GetValue("JShowDbContext", "Data Source=c:\\websites\\WebSpark\\JShow.db");
builder.Services.AddDbContext<JShowDbContext>(options =>
    options.UseSqlite(JShowDbConnectionString));



// ========================
// Identity Configuration
// ========================
builder.Services.AddIdentity<WebSparkUser, IdentityRole>()
    .AddEntityFrameworkStores<WebSparkUserContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddUserManager<ApplicationUserManager>();

// ========================
// HTTP Clients
// ========================
// Base HTTP Client
RegisterHttpClientUtilities(builder);

// ========================
// Add OpenAPI/Swagger generation
//  ========================
builder.Services.AddEndpointsApiExplorer();


// ========================
// OpenWeatherMap Client
// ========================
builder.Services.AddScoped(serviceProvider =>
{
    string apiKey = builder.Configuration["OpenWeatherMapApiKey"] ?? "KEYMISSING";
    var logger = serviceProvider.GetRequiredService<ILogger<WeatherServiceClient>>();
    var loggerLogging = serviceProvider.GetRequiredService<ILogger<WeatherServiceLoggingDecorator>>();
    var loggerCaching = serviceProvider.GetRequiredService<ILogger<WeatherServiceCachingDecorator>>();
    var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

    IOpenWeatherMapClient concreteService = new WeatherServiceClient(apiKey, httpClientFactory, logger);
    IOpenWeatherMapClient withLoggingDecorator = new WeatherServiceLoggingDecorator(concreteService, loggerLogging);
    IOpenWeatherMapClient withCachingDecorator = new WeatherServiceCachingDecorator(withLoggingDecorator, memoryCache, loggerCaching);

    return withCachingDecorator;
});

// ========================
// Application Services
// ========================
builder.Services.AddSingleton<IStringConverter, NewtonsoftJsonStringConverter>();
builder.Services.AddSingleton(new ApplicationStatus(Assembly.GetExecutingAssembly()));
builder.Services.AddSingleton<ChatHistoryStore>();
builder.Services.AddSingleton<IScopeInformation, ScopeInformation>();

// GitHubSpark Services
builder.Services.AddGitHubSparkServices(builder.Configuration);

// Scoped Services
builder.Services.AddScoped<IUserPromptService, UserPromptService>();
builder.Services.AddScoped<IGPTDefinitionService, GPTDefinitionService>();
builder.Services.AddScoped<IGPTDefinitionTypeService, GPTDefinitionTypeService>();
builder.Services.AddScoped<IGPTService, OpenAIChatCompletionService>();
builder.Services.AddScoped<IRecipeGPTService, RecipePromptSparkService>();
builder.Services.AddScoped<IResponseService, ResponseService>();
builder.Services.AddScoped<IWebsiteService, WebsiteProvider>();
builder.Services.AddScoped<IMenuService, MenuProvider>();
builder.Services.AddScoped<IRecipeService, RecipeProvider>();
builder.Services.AddScoped<IMenuProvider, MenuProvider>();
builder.Services.AddScoped<IRecipeImageService, RecipeImageService>();
builder.Services.AddScoped<ICookbook, Cookbook>();
builder.Services.AddScoped<IQuestionSourceAdapter, OpenTriviaDbQuestionSource>();
builder.Services.AddScoped<ITriviaMatchService, TriviaMatchService>();

builder.Services.AddScoped<IJShowService>(serviceProvider =>
{
    string jsonFolder = builder.Configuration["JShow:JsonOutputFolder"] ?? "KEYMISSING";
    JShowConfig config = new() { JsonOutputFolder = jsonFolder };
    // get JShowDbContext from the service provider
    var dbContext = serviceProvider.GetRequiredService<JShowDbContext>();
    JShowService service = new(dbContext, config);
    return service;
});

builder.Services.AddScoped<SiteCrawler, SiteCrawler>();


// Transient Services
builder.Services.AddTransient<CsvProcessingService>();

// ========================
// Markdown Support
// ========================
builder.Services.AddMarkdown();

// ========================
// Bootswatch Theme Switcher
// ========================
builder.Services.AddBootswatchThemeSwitcher();

// ========================
// MVC and Razor Pages
// ========================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add MVC for specific areas
builder.Services.AddMvc()
    .AddApplicationPart(typeof(MarkdownPageProcessorMiddleware).Assembly);

// ========================
// SignalR Configuration
// ========================
// Add CORS configuration if needed for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed(_ => true)  // Allows all origins
               .AllowCredentials();            // Necessary for SignalR
    });
});

// SignalR Configuration
builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    // Configuring JSON serializer options if needed
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});

// ========================
// OpenAI Chat Completion Service
// ========================
string apikey = builder.Configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found";
string modelId = builder.Configuration.GetValue<string>("MODEL_ID") ?? "gpt-4o";
builder.Services.AddOpenAIChatCompletion(modelId, apikey);

builder.Services.Configure<WorkflowOptions>(builder.Configuration.GetSection("Workflow"));
builder.Services.AddSingleton<IWorkflowService, WorkflowService>();

// Configure JsonSerializerOptions
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
});

// Register ConcurrentDictionaryService for Conversation
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<ConversationService>();
// Register PromptSparkHub with all dependencies injected
builder.Services.AddSingleton<PromptSparkHub>();



var app = builder.Build();

// ========================
// Middleware Configuration
// ========================
app.UseMiddleware<NotFoundMiddleware>();



if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// Use all Bootswatch features (includes StyleCache and static files)
app.UseBootswatchAll();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), camera=(), microphone=()");
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Session middleware
app.UseCors("AllowAllOrigins"); // Apply CORS policy for SignalR


// ========================
// Endpoint Configuration
// ========================
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.MapRazorPages();
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapHub<PromptSparkHub>("/promptSparkHub");
app.MapHub<ChatHub>("/chatHub");
app.MapHub<CrawlHub>("/crawlHub");

app.Run();

static void RegisterHttpClientUtilities(WebApplicationBuilder builder)
{
    builder.Services.AddHttpClient("HttpClientService", client =>
    {
        client.Timeout = TimeSpan.FromMilliseconds(90000);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "HttpClientService");
        client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add("X-Request-Source", "HttpClientService");
    });

    // Register IHttpClientMemoryCache implementation
    builder.Services.AddSingleton<IHttpClientMemoryCache, HttpClientMemoryCache>();

    // HTTP Send Service with Decorator Pattern
    builder.Services.AddSingleton(serviceProvider =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var retryOptions = configuration.GetSection("HttpRequestResultPollyOptions").Get<HttpRequestResultPollyOptions>();

        IHttpRequestResultService baseService = new HttpRequestResultService(
            serviceProvider.GetRequiredService<ILogger<HttpRequestResultService>>(),
            configuration,
            serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("HttpClientDecorator"));


        IHttpRequestResultService pollyService = new HttpRequestResultServicePolly(
            serviceProvider.GetRequiredService<ILogger<HttpRequestResultServicePolly>>(),
            baseService,
            retryOptions);

        IHttpRequestResultService telemetryService = new HttpRequestResultServiceTelemetry(
            serviceProvider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
            pollyService);

        IHttpRequestResultService cacheService = new HttpRequestResultServiceCache(
            telemetryService,
            serviceProvider.GetRequiredService<ILogger<HttpRequestResultServiceCache>>(),
            serviceProvider.GetRequiredService<IMemoryCache>());

        return cacheService;
    });
}
