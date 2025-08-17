namespace WebSpark.Core.Infrastructure.Services;

/// <summary>
/// 
/// </summary>
public class StyleService : Interfaces.IStyleProvider
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Models.StyleModel> Get()
    {
        return GetSiteStyles();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Models.StyleModel Get(string name)
    {
        return GetSiteStyles().FirstOrDefault(w => w.name == name) ?? new Models.StyleModel();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static List<Models.StyleModel> GetSiteStyles()
    {
        var siteStyles = new List<Models.StyleModel>();
        try
        {
            siteStyles.Add(new Models.StyleModel()
            {
                name = "mom",
                css = "/style/mom/css/bootstrap.min.css",
                cssMin = "/style/mom/css/bootstrap.min.css",
                cssCdn = "/style/mom/css/bootstrap.min.css"
            });
            siteStyles.Add(new Models.StyleModel()
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
