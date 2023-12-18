namespace ControlSpark.Web.Extensions;

/// <summary>
/// 
/// </summary>
public static class UtilsConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public static string _DefaultSiteId;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceProvider SetUtilsProviderConfiguration(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _DefaultSiteId = configuration.GetValue<string>("DefaultSiteId");
        return serviceProvider;
    }
}
