using WebSpark.Bootswatch.Model;

namespace WebSpark.Bootswatch.Provider;
/// <summary>
/// 
/// </summary>
public class BootswatchStyleProvider : IStyleProvider
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
        return GetSiteStyles()?.Where(w => w.name == name).FirstOrDefault() ?? new StyleModel();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static List<StyleModel> GetSiteStyles()
    {
        var siteStyle = new List<StyleModel>
        {
            new()
            {
                name = "mom",
                css = "/lib/bootstrap/css/bootstrap.min.css",
                cssMin = "/lib/bootstrap/css/bootstrap.min.css",
                cssCdn = "/lib/bootstrap/css/bootstrap.min.css",
                scss = "/lib/bootstrap/scss/bootstrap.scss",
                scssVariables = "/lib/bootstrap/scss/_variables.scss",
                less = "/lib/bootstrap/less/bootstrap.less",
                lessVariables = "/lib/bootstrap/less/variables.less",
                description = "The default Bootstrap theme for mom",
            },
            new()
            {
                name = "texecon",
                css = "/lib/bootstrap/css/bootstrap.min.css",
                cssMin = "/lib/bootstrap/css/bootstrap.min.css",
                cssCdn = "/lib/bootstrap/css/bootstrap.min.css",
                scss = "/lib/bootstrap/scss/bootstrap.scss",
                scssVariables = "/lib/bootstrap/scss/_variables.scss",
                less = "/lib/bootstrap/less/bootstrap.less",
                lessVariables = "/lib/bootstrap/less/variables.less",
                description = "The default Bootstrap theme for texecon",
            }
        };
        try
        {
            var myClient = new HttpClient();
            var task = Task.Run(() => myClient.GetFromJsonAsync<BootswatchResponse>("http://bootswatch.com/api/5.json"));
            task.Wait();
            task.Result?.themes?.ForEach(myTheme => { siteStyle.Add(Create(myTheme)); });
        }
        catch
        {
        }
        return siteStyle;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="theme"></param>
    /// <returns></returns>
    private static StyleModel Create(BootswatchStyle theme)
    {
        return new StyleModel()
        {
            scss = theme.scss,
            scssVariables = theme.scssVariables,
            css = theme.css,
            cssCdn = theme.cssCdn,
            cssMin = theme.cssMin,
            description = theme.description,
            less = theme.less,
            lessVariables = theme.lessVariables,
            name = theme.name,
            preview = theme.preview,
            thumbnail = theme.thumbnail

        };

    }
}

