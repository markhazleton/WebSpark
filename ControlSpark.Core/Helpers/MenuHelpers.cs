namespace ControlSpark.Core.Helpers;

/// <summary>
/// Menu Helper
/// </summary>
public static class MenuHelpers
{
    /// <summary>
    /// Gets the bread crumb.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="theModel">The model.</param>
    /// <returns>List&lt;LinkModel&gt;.</returns>
    public static List<LinkModel> GetBreadCrumb(this HtmlHelper html, WebsiteVM theModel)
    {
        var myList = new List<LinkModel>();
        var myMenu = new MenuModel();

        if (html == null) return myList;

        var curPath = html.ViewContext.HttpContext.Request.Path.Value
            .ToLower(CultureInfo.CurrentCulture)
            .Split('/');
        foreach (var path in curPath.Where(w => w.ToLower(CultureInfo.CurrentCulture) != "index"))
        {
            if (string.Compare(path.Trim().ToLower(CultureInfo.CurrentCulture),
                              string.Empty,
                              StringComparison.Ordinal) ==
                0 &&
                myList.Count == 0)
            {
                myList.Add(new LinkModel() { Href = "/", Method = "Home" });
            }
            else
            {
                if (myList.Where(w => w.Method.ToLower(CultureInfo.CurrentCulture) ==
                         path.ToLower(CultureInfo.CurrentCulture))
                        .FirstOrDefault() ==
                    null)
                {
                    myMenu = theModel
                    .Menu
                        .Where(w => w.DomainID == theModel.WebsiteId)
                        .Where(w => (w.Action.ToLower(CultureInfo.CurrentCulture).Trim() ==
                                path.ToLower(CultureInfo.CurrentCulture).Trim()) ||
                            w.Controller.ToLower(CultureInfo.CurrentCulture).Trim() ==
                            path.ToLower(CultureInfo.CurrentCulture).Trim())
                        .FirstOrDefault();
                    if (myMenu != null)
                    {
                        if (myList.Where(w => w.Method == myMenu.Title).FirstOrDefault() == null)
                        {
                            myList.Add(new LinkModel() { Href = $"/{myMenu.Url}", Method = myMenu.Title });
                        }
                    }
                }
            }
        }

        var localPath = html.ViewContext.HttpContext.Request.Path.Value.ToLower(CultureInfo.CurrentCulture);
        myMenu = theModel.Menu
            .Where(w => w.DomainID == theModel.WebsiteId)
            .Where(w => $"/{w.Url}" == localPath)
            .FirstOrDefault();
        if (myMenu != null)
        {
            if (myList.Where(w => w.Method == myMenu.Title).FirstOrDefault() == null)
            {
                myList.Add(new LinkModel() { Href = $"/{myMenu.Url}", Method = myMenu.Title });
            }
        }
        return myList;
    }


}
