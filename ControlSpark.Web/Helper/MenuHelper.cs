using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlSpark.Web.Helper;

/// <summary>
/// 
/// </summary>
public static class MenuHelper
{

    /// <summary>
    /// Gets the bread crumb.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="theModel">The model.</param>
    /// <returns>List&lt;LinkModel&gt;.</returns>
    public static List<LinkModel> GetBreadCrumb(this IHtmlHelper html, WebsiteVM theModel)
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
                            myList.Add(new LinkModel() { Href = $"{myMenu.Url}", Method = myMenu.Title });
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
                myList.Add(new LinkModel() { Href = $"{myMenu.Url}", Method = myMenu.Title });
            }
        }
        return myList;
    }
    /// <summary>
    /// IsActive
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="item">The item.</param>
    /// <param name="IsParent">if set to <c>true</c> [is parent].</param>
    /// <returns>System.String.</returns>
    public static string IsActive(this IHtmlHelper html, MenuModel item, bool IsParent)
    {
        if (html == null)
            return string.Empty;

        if (item == null)
            return string.Empty;

        var curPath = html.ViewContext.HttpContext.Request.Path.Value
            .ToLower(CultureInfo.CurrentCulture)
            .Split('/');
        if (curPath.Length <= 2)
            return string.Empty;
        if (item.Controller.ToLower(CultureInfo.CurrentCulture) != "page")
        {
            if (curPath.Length == 3)
            {
                var curItem = ((WebsiteVM)html.ViewData.Model).Menu
                    .Where(w => w.Action.ToLower(CultureInfo.CurrentCulture) == curPath[2])
                    .Where(w => w.Controller.ToLower(CultureInfo.CurrentCulture) == curPath[1])
                    .FirstOrDefault();

                if (curPath[1] == item.Controller.ToLower(CultureInfo.CurrentCulture) &&
                    curPath[2] == item.Action.ToLower(CultureInfo.CurrentCulture))
                    return "active";
            }
        }
        else
        {
            if (curPath.Length == 3 && IsParent)
            {
                var curItem = ((WebsiteVM)html.ViewData.Model).Menu
                    .Where(w => w.Action.ToLower(CultureInfo.CurrentCulture) == curPath[2])
                    .Where(w => w.Controller.ToLower(CultureInfo.CurrentCulture) == curPath[1])
                    .FirstOrDefault();

                if (curItem != null)
                    if (curItem.ParentTitle != null)
                        if (item.Action.ToLower(CultureInfo.CurrentCulture) ==
                            curItem.ParentTitle.ToLower(CultureInfo.CurrentCulture))
                            return "active";
            }
            if (curPath.Length > 1 && IsParent && curPath[1] == item.Action.ToLower(CultureInfo.CurrentCulture))
                return "active";

            if (curPath.Length == 2 && !IsParent && curPath[1] == item.Action.ToLower(CultureInfo.CurrentCulture))
                return "active";

            if (curPath.Length > 2 && !IsParent && curPath[2] == item.Action.ToLower(CultureInfo.CurrentCulture))
                return "active";
        }
        return string.Empty;
    }


    /// <summary>
    /// IsActive
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="control">The control.</param>
    /// <param name="action">The action.</param>
    /// <returns>System.String.</returns>
    public static string IsActive(this IHtmlHelper html, string control, string action)
    {
        if (html == null)
            return string.Empty;

        var routeData = html.ViewContext.RouteData;
        var routeAction = (string)routeData.Values["action"];
        var routeControl = (string)routeData.Values["controller"];

        // both must match
        var returnActive = control == routeControl && action == routeAction;
        return returnActive ? "active" : string.Empty;
    }
}
