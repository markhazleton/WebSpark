using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptSpark.Chat.Hubs;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Register services
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddOpenApi();

// Configure a named HttpClient called "workflow" with a base URL
builder.Services.AddHttpClient("workflow", client =>
{
    var urls = builder.Configuration.GetValue<string>("ASPNETCORE_URLS");
    var defaultHost = "localhost";
    var defaultPort = 7105;

    // Check if ASPNETCORE_URLS contains a full URL, and extract the host if so
    var uriBuilder = new UriBuilder
    {
        Scheme = "https",
        Host = defaultHost,
        Port = builder.Configuration.GetValue("ASPNETCORE_HTTPS_PORT", defaultPort),
        Path = "api/"
    };

    if (!string.IsNullOrEmpty(urls))
    {
        if (!string.IsNullOrEmpty(urls))
        {
            try
            {
                var firstUrl = urls.Split(';').FirstOrDefault();
                if (!string.IsNullOrEmpty(firstUrl))
                {
                    var uri = new Uri(firstUrl);
                    uriBuilder.Host = uri.Host;
                    uriBuilder.Port = uri.Port;
                }
            }
            catch (UriFormatException)
            {
                // Log or handle URI parse error if needed
            }
        }
    }
    client.BaseAddress = uriBuilder.Uri;
});


// ========================
// OpenAI Chat Completion Service
// ========================
string apikey = builder.Configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found";
string modelId = builder.Configuration.GetValue<string>("MODEL_ID") ?? "gpt-4o";
builder.Services.AddOpenAIChatCompletion(modelId, apikey);

// Register ChatHub with workflow JSON as a parameter
builder.Services.AddSingleton<ChatHub>(provider =>
{
    JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "workflow.json");
    var jsonTemplate = System.IO.File.ReadAllText(jsonPath);
    var logger = provider.GetRequiredService<ILogger<ChatHub>>();
    var chatCompletionService = provider.GetRequiredService<IChatCompletionService>();
    return new ChatHub(logger, chatCompletionService, jsonTemplate);
});

var app = builder.Build();

// Middleware setup
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapOpenApi();
app.MapScalarApiReference();

// Top-level route registrations (replacing UseEndpoints)
app.MapControllers(); // Maps all API and MVC controllers automatically
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hubs
app.MapHub<ChatHub>("/chatHub");

app.Run();
