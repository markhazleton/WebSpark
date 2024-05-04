using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WebSpark.Core.Data;

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
        using AppDbContext cmsCtx = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        if (!cmsCtx.Database.CanConnect())
        {
            await cmsCtx.Database.EnsureDeletedAsync();
            await cmsCtx.Database.EnsureCreatedAsync();
            var seedDatabase = new SeedDatabase(cmsCtx);
            await seedDatabase.SeedDatabaseAsync();
        }

    }
}
