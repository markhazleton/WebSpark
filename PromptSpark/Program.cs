using HttpClientUtility.FullService;
using HttpClientUtility.StringConverter;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using PromptSpark.Areas.Identity.Data;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Service;
using PromptSpark.Utilities;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "logging.db");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.SQLite(logFilePath)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure services
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var adminConnectionString = builder.Configuration.GetConnectionString("AdminContext");
builder.Services.AddDbContext<AdminContext>(options =>
    options.UseSqlite(adminConnectionString));

builder.Services.AddIdentity<AdminUser, IdentityRole>()
        .AddEntityFrameworkStores<AdminContext>()
        .AddDefaultUI()
        .AddDefaultTokenProviders()
        .AddUserManager<ApplicationUserManager>();

var GPTDbConnectionString = builder.Configuration.GetConnectionString("GPTDbContext");
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

var app = builder.Build();

// Migrate database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userCtx = services.GetRequiredService<AdminContext>();
    userCtx.Database.Migrate();
}

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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "App_Data")),
    RequestPath = "/App_Data",
    ServeUnknownFileTypes = true,
    DefaultContentType = "text/plain"
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Add this line

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
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.Run();

// Ensure to flush and close the log when the application shuts down
Log.CloseAndFlush();
