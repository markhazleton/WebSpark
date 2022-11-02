using ControlSpark.Core.Data;
using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.SwaggerCore.Extensions;
using ControlSpark.WebMvc.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", false, true)
                            .AddEnvironmentVariables()
                            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                            .Build();
Log.Logger = new LoggerConfiguration()
      .Enrich.FromLogContext()
      .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
      .CreateLogger();

var connectionString = builder.Configuration.GetConnectionString("ControlSparkUserContextConnection") ?? throw new InvalidOperationException("Connection string 'ControlSparkUserContextConnection' not found.");

builder.Services.AddDbContext<ControlSparkUserContext>(options =>
    options.UseSqlite(connectionString)); ;

builder.Services.AddDefaultIdentity<ControlSparkUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ControlSparkUserContext>(); ;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration["ConnectionStrings:DefaultConnection"]);
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerServices(builder.Configuration);

builder.Services.AddSingleton<IScopeInformation, ScopeInformation>();
builder.Services.AddScoped<IWebsiteService, WebsiteProvider>();
builder.Services.AddScoped<IMenuService, MenuProvider>();
builder.Services.AddScoped<IRecipeService, RecipeProvider>();
builder.Services.AddScoped<IMenuProvider, MenuProvider>();
builder.Services.AddBlogDatabase(config);
builder.Services.AddBlogProviders();


var app = builder.Build();

// Initialize User Db (if necessary)
await UserDbInitializer.SeedAsync(app);
// Initialize CMS Db (if necessary)
await DbInitializer.SeedAsync(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSwaggerWithVersioning();
app.UseRouting();
app.UseAuthentication(); ;

app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
