using WebSpark.Domain.Interfaces;
using WebSpark.Domain.Models;

namespace WebSpark.Core.Infrastructure.Services;

/// <summary>
/// 
/// </summary>
public class StyleService : IStyleProvider
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<StyleModel> Get()
    {
        return GetSiteStyles();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public StyleModel Get(string name)
    {
        return GetSiteStyles().Where(w => w.name == name).FirstOrDefault();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static List<StyleModel> GetSiteStyles()
    {
        var siteStyles = new List<StyleModel>();
        try
        {
            siteStyles.Add(new StyleModel()
            {
                name = "mom",
                css = "/style/mom/css/bootstrap.min.css",
                cssMin = "/style/mom/css/bootstrap.min.css",
                cssCdn = "/style/mom/css/bootstrap.min.css"
            });
            siteStyles.Add(new StyleModel()
            {
                name = "texecon",
                css = "/style/texecon/css/bootstrap.min.css",
                cssMin = "/style/texecon/css/bootstrap.min.css",
                cssCdn = "/style/texecon/css/bootstrap.min.css"
            });
        }
        catch
        {
        }
        return siteStyles;
    }
}
