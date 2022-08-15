
namespace ControlSpark.Core.Infrastructure;

/// <summary>
/// 
/// </summary>
public static class FormatHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetSafePath(string name)
    {
        return name == null
            ? string.Empty
            : $"{(name.Replace("&", "-").Replace("\n", string.Empty).Replace("/", "-").Replace("'", "-").Replace(" ", "-").ToLower(CultureInfo.CurrentCulture))}";
    }
    //public static string GetSafePath(string name)
    //{
    //    return name == null
    //        ? string.Empty
    //        : $"{ (name.Replace("&", "-").Replace("/", "-").Replace("'", "-").Replace(" ", "-").ToLower(CultureInfo.CurrentCulture))}";
    //}
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    public static string GetSafePath(string name, string root)
    {
        return $"{GetSafePath(root)}{GetSafePath(name)}";
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="recipeName"></param>
    /// 
    /// <returns></returns>
    public static string GetRecipeURL(string recipeName)
    {
        return $"/recipe/{FormatHelper.GetSafePath(recipeName)}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetRecipeCategoryURL(string name)
    {
        return $"/recipe/category/{FormatHelper.GetSafePath(name)}";
    }
}
