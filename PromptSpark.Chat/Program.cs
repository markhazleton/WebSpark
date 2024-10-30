using Microsoft.SemanticKernel;
using PromptSpark.Chat.Hubs;
using Scalar.AspNetCore;

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
        try
        {
            var uri = new Uri(urls.Split(';').FirstOrDefault());
            uriBuilder.Host = uri.Host;
            uriBuilder.Port = uri.Port;
        }
        catch (UriFormatException)
        {
            // Log or handle URI parse error if needed
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
app.MapHub<WorkflowHub>("/workflowHub");

app.Run();
