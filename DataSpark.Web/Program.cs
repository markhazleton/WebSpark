using DataSpark.Web.Services;
using WebSpark.Bootswatch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DataSpark services
builder.Services.AddScoped<CsvFileService>();
builder.Services.AddScoped<CsvProcessingService>();

// Add memory cache services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<WebSpark.HttpClientUtility.MemoryCache.IMemoryCacheManager, WebSpark.HttpClientUtility.MemoryCache.MemoryCacheManager>();

// Add Bootswatch theme switcher services (includes StyleCache)
builder.Services.AddBootswatchThemeSwitcher();

// Register IHttpContextAccessor as required by Bootswatch for theme switching tag helper
builder.Services.AddHttpContextAccessor();

// Register WebSpark.HttpClientUtility services required by Bootswatch
builder.Services.AddScoped<WebSpark.HttpClientUtility.RequestResult.IHttpRequestResultService, WebSpark.HttpClientUtility.RequestResult.HttpRequestResultService>();

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

// Use all Bootswatch features (includes StyleCache and static files)
app.UseBootswatchAll();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
