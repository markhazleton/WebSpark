using WebSpark.Domain.Models;

namespace WebSpark.Domain.Interfaces;

/// <summary>
/// Style Service
/// </summary>
public interface IStyleProvider
{
    /// <summary>
    /// Get List of Themes
    /// </summary>
    /// <returns></returns>
    IEnumerable<StyleModel> Get();
    /// <summary>
    /// Get User By Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    StyleModel Get(string id);
}
