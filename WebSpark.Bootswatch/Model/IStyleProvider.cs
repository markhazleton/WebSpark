
namespace WebSpark.Bootswatch.Model;

/// <summary>
/// Style Service
/// </summary>
public interface IStyleProvider
{
    /// <summary>
    /// Get List of Themes
    /// </summary>
    /// <returns></returns>
    IEnumerable<BootswatchStyleModel> Get();
    /// <summary>
    /// Get User By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    BootswatchStyleModel Get(string id);
}
