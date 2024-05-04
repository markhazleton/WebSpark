using System.Text.Json;

namespace WebSpark.Web.Extensions;

/// <summary>
/// 
/// </summary>
public static class SessionExtensionsKeys
{
    /// <summary>
    /// 
    /// </summary>
    public const string SessionInitialized = "SessionInitialized";
    /// <summary>
    /// 
    /// </summary>
    public const string BaseViewKey = "BaseViewKey";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="session"></param>
    /// <param name="context"></param>
    public static Task InitializeAsync(this ISession session, HttpContext context)
    {
        if (session.GetInt32(SessionInitialized) == 1)
        {
            return Task.CompletedTask;
        }
        session.SetInt32(SessionInitialized, 1);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="session"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="session"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
