using HttpClientUtility;
using HttpClientUtility.FullService;
using HttpClientUtility.StringConverter;
using Microsoft.EntityFrameworkCore;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Service;
using WebSpark.Core.Data;
using WebSpark.Core.Extensions;
using WebSpark.Core.Providers;
using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;
using WebSpark.RecipeManager.Interfaces;
using Westwind.AspNetCore.Markdown;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>();

WebSpark.Core.Infrastructure.Logging.LoggingUtility.ConfigureLogging(builder, "WebSpark.Admin");

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var AppDbConnectionString = builder.Configuration.GetValue("WebSparkContext", "Data Source=c:\\websites\\WebSpark\\webspark.db");
builder.Services.AddDbContext<AppDbContext>(options =>
{
  options.UseSqlite(AppDbConnectionString);
});

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
builder.Services.AddScoped<IRecipeGPTService, RecipePromptSparkService>();

builder.Services.AddMarkdown();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IScopeInformation, ScopeInformation>();
builder.Services.AddScoped<IWebsiteService, WebsiteProvider>();
builder.Services.AddScoped<IMenuService, MenuProvider>();
builder.Services.AddScoped<IRecipeService, RecipeProvider>();
builder.Services.AddScoped<IMenuProvider, MenuProvider>();
builder.Services.AddScoped<IRecipeImageService, RecipeImageService>();
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
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
  endpoints.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
  );
  endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

});

app.Run();
