using HttpClientUtility.MemoryCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using OpenWeatherMapClient.Interfaces;
using OpenWeatherMapClient.WeatherService;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.PromptSparkChat;
using PromptSpark.Domain.Service;
using Scalar.AspNetCore;
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
using WebSpark.Portal.Utilities;
using System.Text.RegularExpressions;

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
// ========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();


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
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        // Tightened: restrict to configured origins (semicolon separated) or sensible localhost defaults
        var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? builder.Configuration.GetValue<string>("AllowedOrigins")?
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? new[] { "https://localhost:5001", "https://localhost:7123" };
        policy.WithOrigins(configuredOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Needed for SignalR auth/cookies
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

// Conversation logging service (async background CSV writer)
builder.Services.AddSingleton<ConversationLogService>();
builder.Services.AddSingleton<IConversationLogService>(sp => sp.GetRequiredService<ConversationLogService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<ConversationLogService>());



var app = builder.Build();

// ========================
// Middleware Configuration
// (NotFoundMiddleware moved later after status code pages & before endpoints)
// ========================



if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // OpenAPI endpoint mapping - must be before UseRouting()
    app.MapOpenApi();

    // Add Scalar UI for modern API documentation
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("WebSpark Portal API")
               .WithTheme(ScalarTheme.BluePlanet)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .WithModels(true)
               .WithSearchHotKey("k");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// =====================================
// Combined Middleware: Buffer -> Hash inline scripts -> Set Security Headers + CSP -> Write Body
// =====================================
app.Use(async (context, next) =>
{
    var env = app.Environment;
    var originalBody = context.Response.Body;
    await using var buffer = new MemoryStream();
    context.Response.Body = buffer;

    await next(); // execute rest of pipeline writing into buffer

    buffer.Seek(0, SeekOrigin.Begin);
    var isHtml = context.Response.ContentType != null && context.Response.ContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    List<string> scriptHashes = new();
    string? html = null;
    if (isHtml)
    {
        html = await new StreamReader(buffer).ReadToEndAsync();
        var inlineScriptPattern = new Regex("<script(?![^>]*\\bsrc=)(?![^>]*type=\"application/ld\\+json\")[^>]*>([\\s\\S]*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var matches = inlineScriptPattern.Matches(html);
        foreach (System.Text.RegularExpressions.Match m in matches)
        {
            var code = m.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(code)) continue; // keep whitespace inside hash, so skip only fully blank
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(code));
            var base64 = Convert.ToBase64String(hashBytes);
            scriptHashes.Add($"'sha256-{base64}'");
        }
    }

    // ---------- Security Headers ----------
    var headers = context.Response.Headers;
    void SetHeader(string key, string value)
    {
        if (!headers.ContainsKey(key)) headers.Append(key, value);
    }
    SetHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    SetHeader("X-Content-Type-Options", "nosniff");
    SetHeader("X-Frame-Options", "DENY");
    SetHeader("Referrer-Policy", "strict-origin-when-cross-origin");
    SetHeader("Permissions-Policy", "geolocation=(), camera=(), microphone=()");
    SetHeader("Cross-Origin-Opener-Policy", "same-origin");
    SetHeader("Cross-Origin-Embedder-Policy", "require-corp");
    SetHeader("Cross-Origin-Resource-Policy", "same-origin");

    // ---------- CSP Construction ----------
    var scriptSrc = new List<string> { "'self'" };
    var styleSrc = new List<string> { "'self'", "https://cdn.jsdelivr.net", "'unsafe-inline'", "https://fonts.googleapis.com" }; // retain inline styles for now
    var fontSrc = new List<string> { "'self'", "data:", "https://cdn.jsdelivr.net", "https://fonts.gstatic.com" };
    var connectSrc = new List<string> { "'self'", "https:", "wss:", "ws:" }; // add ws: for BrowserLink / dev websockets
    var imgSrc = new List<string> { "'self'", "data:", "blob:" };

    scriptSrc.AddRange(new[]
    {
        "https://www.googletagmanager.com",
        "https://www.google-analytics.com",
        "https://cdn.jsdelivr.net",
        "https://cdnjs.cloudflare.com",
        "https://unpkg.com",
        "https://ajax.aspnetcdn.com",
        "https://code.jquery.com",
        "https://cdn.plot.ly"
    });
    connectSrc.Add("https://www.google-analytics.com");

    if (env.IsDevelopment())
    {
        connectSrc.Add("http://localhost:*");
    }

    if (scriptHashes.Count > 0)
    {
        scriptSrc.AddRange(scriptHashes);
    }

    scriptSrc = scriptSrc.Distinct().ToList();
    styleSrc = styleSrc.Distinct().ToList();
    fontSrc = fontSrc.Distinct().ToList();
    connectSrc = connectSrc.Distinct().ToList();
    imgSrc = imgSrc.Distinct().ToList();

    // Allow embedding YouTube videos (iframe) and required thumbnail domains
    var frameSrc = new List<string>
    {
        "'self'",
        "https://www.youtube.com",
        "https://www.youtube-nocookie.com"
    };
    // YouTube thumbnails
    if (!imgSrc.Contains("https://i.ytimg.com")) imgSrc.Add("https://i.ytimg.com");
    imgSrc = imgSrc.Distinct().ToList();

    var csp = string.Join("; ", new[]
    {
        $"default-src 'self'",
        $"script-src {string.Join(' ', scriptSrc)}",
        $"style-src {string.Join(' ', styleSrc)}",
        $"img-src {string.Join(' ', imgSrc)}",
        $"font-src {string.Join(' ', fontSrc)}",
        $"connect-src {string.Join(' ', connectSrc)}",
    $"frame-src {string.Join(' ', frameSrc)}",
        "frame-ancestors 'none'",
        "object-src 'none'",
        "base-uri 'self'",
        "form-action 'self'"
    });
    if (!headers.ContainsKey("Content-Security-Policy"))
        headers.Append("Content-Security-Policy", csp);

    // ---------- Write Body ----------
    buffer.Seek(0, SeekOrigin.Begin);
    if (isHtml && html != null)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
        context.Response.Headers.Remove("Content-Length");
        context.Response.Body = originalBody;
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
    else
    {
        await buffer.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }
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
app.UseMiddleware<NotFoundMiddleware>();
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
            serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("HttpClientService"));


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
