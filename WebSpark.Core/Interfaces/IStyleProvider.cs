namespace WebSpark.Core.Interfaces;

/// <summary>
/// Style Service
/// </summary>
public interface IStyleProvider
{
    /// <summary>
    /// Get List of Themes
    /// </summary>
    /// <returns></returns>
    IEnumerable<Models.StyleModel> Get();
    /// <summary>
    /// Get User By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Models.StyleModel Get(string id);
}
