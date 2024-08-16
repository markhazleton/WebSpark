
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WebSpark.Core.Extensions;

/// <summary>
/// 
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public static void AddHttpContextAccessor(this IServiceCollection services)
    { services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
    {
        MyHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
        return app;
    }
}
