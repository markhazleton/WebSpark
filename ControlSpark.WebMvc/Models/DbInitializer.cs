using ControlSpark.Core.Data;
using ControlSpark.Core.Helpers;
using ControlSpark.WebMvc.Areas.Identity.Data;

namespace ControlSpark.WebMvc.Models;

/// <summary>
/// Db Initializer
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seed the Db if needed
    /// </summary>
    /// <param name="applicationBuilder"></param>
    public static async Task SeedAsync(IApplicationBuilder applicationBuilder)
    {
        using ControlSparkUserContext userCtx = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<ControlSparkUserContext>();
        await userCtx.Database.EnsureDeletedAsync();
        await userCtx.Database.EnsureCreatedAsync();

        using AppDbContext cmsCtx = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        await cmsCtx.Database.EnsureDeletedAsync();
        await cmsCtx.Database.EnsureCreatedAsync();
        var seedDatabase = new SeedDatabase(cmsCtx);
        await seedDatabase.SeedDatabaseAsync();

    }
}
