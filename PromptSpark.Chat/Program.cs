using Microsoft.SemanticKernel;
using PromptSpark.Chat.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug); // for more detailed logs

// Add SignalR
builder.Services.AddSignalR();

// Add controllers if you have API endpoints
builder.Services.AddControllers();

// ========================
// OpenAI Chat Completion Service
// ========================
string apikey = builder.Configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found";
string modelId = builder.Configuration.GetValue<string>("MODEL_ID") ?? "gpt-4o";
builder.Services.AddOpenAIChatCompletion(modelId, apikey);

var app = builder.Build();
app.UseDefaultFiles();  // Looks for index.html, index.htm by default
app.UseStaticFiles();

// Configure routing
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
