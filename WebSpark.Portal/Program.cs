using HttpClientCrawler.Crawler;
using HttpClientUtility.FullService;
using HttpClientUtility.GetService;
using HttpClientUtility.MemoryCache;
using HttpClientUtility.SendService;
using HttpClientUtility.StringConverter;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using OpenWeatherMapClient.Interfaces;
using OpenWeatherMapClient.WeatherService;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Service;
using Serilog;
using System.Reflection;
using WebSpark.Core.Data;
using WebSpark.Core.Infrastructure.Logging;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;
using WebSpark.Core.Providers;
using WebSpark.Portal.Areas.DataSpark.Services;
using WebSpark.RecipeCookbook;
using Westwind.AspNetCore.Markdown;

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
// OpenWeatherMap Client
// ========================
builder.Services.AddScoped<IOpenWeatherMapClient>(serviceProvider =>
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

// Transient Services
builder.Services.AddTransient<CsvProcessingService>();

// ========================
// Markdown Support
// ========================
builder.Services.AddMarkdown();

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

var app = builder.Build();

// ========================
// Middleware Configuration
// ========================
// Middleware to enforce lowercase routes
//app.Use(async (context, next) =>
//{
//    var request = context.Request;
//    var path = request.Path.Value;

//    // Check if the path contains any uppercase characters
//    if (path != null && path.Any(char.IsUpper))
//    {
//        // Convert the path to lowercase
//        var lowercasePath = path.ToLowerInvariant();

//        // Construct the new URL with the lowercase path
//        var newUrl = $"{request.Scheme}://{request.Host}{lowercasePath}{request.QueryString}";

//        // Redirect to the lowercase URL with a 301 (Permanent Redirect) status code
//        context.Response.Redirect(newUrl, true);
//        return;
//    }
//    await next();
//});



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

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    // context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'; object-src 'none'; frame-ancestors 'none';");
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
app.MapRazorPages();
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapHub<ChatHub>("/chatHub");
app.MapHub<CrawlHub>("/crawlHub");


// Ensure to flush and close the log when the application shuts down
Log.CloseAndFlush();

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

    // Full Service HTTP Client with Telemetry
    builder.Services.AddScoped(serviceProvider =>
    {
        IHttpClientFullService baseService = new HttpClientFullService(
            serviceProvider.GetRequiredService<IHttpClientFactory>(),
            serviceProvider.GetRequiredService<IStringConverter>());

        IHttpClientFullService telemetryService = new HttpClientFullServiceTelemetry(baseService);

        return telemetryService;
    });

    // HTTP Get Call Service with Decorator Pattern
    builder.Services.AddScoped(serviceProvider =>
    {
        var logger = serviceProvider.GetRequiredService<ILogger<HttpGetCallService>>();
        var telemetryLogger = serviceProvider.GetRequiredService<ILogger<HttpGetCallServiceTelemetry>>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        IHttpGetCallService baseService = new HttpGetCallService(logger, httpClientFactory);
        IHttpGetCallService telemetryService = new HttpGetCallServiceTelemetry(telemetryLogger, baseService);

        return telemetryService;
    });

    // HTTP Send Service with Decorator Pattern
    builder.Services.AddSingleton(serviceProvider =>
    {
        IHttpClientSendService baseService = new HttpClientSendService(
            serviceProvider.GetRequiredService<ILogger<HttpClientSendService>>(),
            serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("HttpClientDecorator"));

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var retryOptions = configuration.GetSection("HttpClientSendPollyOptions").Get<HttpClientSendPollyOptions>();

        IHttpClientSendService pollyService = new HttpClientSendServicePolly(
            serviceProvider.GetRequiredService<ILogger<HttpClientSendServicePolly>>(),
            baseService,
            retryOptions);

        IHttpClientSendService telemetryService = new HttpClientSendServiceTelemetry(
            serviceProvider.GetRequiredService<ILogger<HttpClientSendServiceTelemetry>>(),
            pollyService);

        IHttpClientSendService cacheService = new HttpClientSendServiceCache(
            telemetryService,
            serviceProvider.GetRequiredService<ILogger<HttpClientSendServiceCache>>(),
            serviceProvider.GetRequiredService<IMemoryCache>());

        return cacheService;
    });
}
