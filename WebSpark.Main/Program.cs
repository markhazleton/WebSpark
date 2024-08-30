using HttpClientUtility.FullService;
using HttpClientUtility.GetService;
using HttpClientUtility.MemoryCache;
using HttpClientUtility.SendService;
using HttpClientUtility.StringConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using OpenWeatherMapClient.Interfaces;
using OpenWeatherMapClient.WeatherService;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Service;
using ScottPlot.Statistics;
using Serilog;
using WebSpark.Core.Infrastructure.Logging;
using WebSpark.Main.Areas.DataSpark.Services;
using WebSpark.Main.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>();

LoggingUtility.ConfigureLogging(builder, "WebSparkMain");
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<IMemoryCacheManager, MemoryCacheManager>();

var GPTDbConnectionString = builder.Configuration.GetValue("GPTDbContext", "Data Source=c:\\websites\\WebSpark\\PromptSpark.db");
builder.Services.AddDbContext<GPTDbContext>(options =>
    options.UseSqlite(GPTDbConnectionString));

builder.Services.AddSingleton<IStringConverter, NewtonsoftJsonStringConverter>();
builder.Services.AddHttpClient("HttpClientService", client =>
{
    client.Timeout = TimeSpan.FromMilliseconds(90000);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientService");
    client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
    client.DefaultRequestHeaders.Add("X-Request-Source", "HttpClientService");
});

// Register the HttpGetCallService with Decorator Pattern
builder.Services.AddScoped(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<HttpGetCallService>>();
    var telemetryLogger = serviceProvider.GetRequiredService<ILogger<HttpGetCallServiceTelemetry>>();
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    IHttpGetCallService baseService = new HttpGetCallService(logger, httpClientFactory);
    IHttpGetCallService telemetryService = new HttpGetCallServiceTelemetry(telemetryLogger, baseService);
    return telemetryService;
});


// Register the OpenWeatherMapClient with Decorator Pattern
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

// Register the HttpClientSendService with Decorator Pattern
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

// Register the HttpClientFullService with Decorator Pattern
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

// Register the OpenAIChatCompletionService
string apikey = builder.Configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found";
string modelId = builder.Configuration.GetValue<string>("MODEL_ID") ?? "gpt-4o";
builder.Services.AddOpenAIChatCompletion(modelId, apikey);
builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    // Configuring JSON serializer options if needed
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddSingleton<ChatHistoryStore>();
builder.Services.AddScoped<IUserPromptService, UserPromptService>();
builder.Services.AddScoped<IGPTDefinitionService, GPTDefinitionService>();
builder.Services.AddScoped<IGPTDefinitionTypeService, GPTDefinitionTypeService>();
builder.Services.AddScoped<IGPTService, OpenAIChatCompletionService>();

// Data Spark services
builder.Services.AddTransient<CsvProcessingService>();


builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

// Add OpenAPI/Swagger documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebSpark API",
        Version = "v1",
        Description = "API documentation for WebSpark"
    });
});



var app = builder.Build();

// Configure middleware
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
app.UseStaticFiles(); // For serving static files
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Add this line

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Serve ReDoc UI
app.UseReDoc(c =>
{
    c.RoutePrefix = "api-docs"; // URL at which the ReDoc UI will be available
    c.SpecUrl("/swagger/v1/swagger.json"); // The URL where the Swagger JSON file will be available
    c.DocumentTitle = "WebSpark API Documentation";
});

// Configure endpoints
app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areaRoute",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
        );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
        );

    endpoints.MapHub<ChatHub>("/chatHub");

});

app.Run();

// Ensure to flush and close the log when the application shuts down
Log.CloseAndFlush();
