using ControlSpark.WebMvc.Areas.Identity.Data;

namespace ControlSpark.Core.Data;

/// <summary>
/// Db Initializer
/// </summary>
public static class UserDbInitializer
{
    /// <summary>
    /// Seed the Db if needed
    /// </summary>
    /// <param name="applicationBuilder"></param>
    public static async Task SeedAsync(IApplicationBuilder applicationBuilder)
    {
        using ControlSparkUserContext userCtx = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<ControlSparkUserContext>();
        if (!userCtx.Database.CanConnect())
        {
            await userCtx.Database.EnsureDeletedAsync();
            await userCtx.Database.EnsureCreatedAsync();
        }
    }
}
