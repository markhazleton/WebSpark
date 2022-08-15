using ControlSpark.Bootswatch.Model;
using ControlSpark.Domain.Interfaces;
using ControlSpark.Domain.Models;

namespace ControlSpark.Bootswatch.Provider;
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
        return GetSiteThemes();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public StyleModel Get(string name)
    {
        return GetSiteThemes()?.Where(w => w.name == name).FirstOrDefault() ?? new StyleModel();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<StyleModel> GetSiteThemes()
    {
        var siteStyle = new List<StyleModel>
            {
                new StyleModel()
                {
                    name = "mom",
                    css = "/style/mom/css/bootstrap.min.css",
                    cssMin = "/style/mom/css/bootstrap.min.css",
                    cssCdn = "/style/mom/css/bootstrap.min.css"
                },
            new StyleModel()
                {
                    name = "texecon",
                    css = "/style/texecon/css/bootstrap.min.css",
                    cssMin = "/style/texecon/css/bootstrap.min.css",
                    cssCdn = "/style/texecon/css/bootstrap.min.css"
                }
            };
        try
        {
            var myClient = new HttpClient();
            var task = Task.Run(() => myClient.GetFromJsonAsync<Model.Bootswatch>("http://bootswatch.com/api/5.json"));
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

