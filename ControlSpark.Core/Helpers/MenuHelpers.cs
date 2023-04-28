namespace ControlSpark.Core.Helpers;

/// <summary>
/// Menu Helper
/// </summary>
public static class MenuHelpers
{

    /// <summary>
    /// GetBaseViewByDomain
    /// </summary>
    /// <param name="HostDomain">The host domain.</param>
    /// <returns>BaseViewModel.</returns>
    public static WebsiteVM GetBaseViewByDomain(string HostDomain, AppDbContext dbContext)
    {
        var BaseView = new WebsiteVM();
        switch (HostDomain)
        {
            case "mwhcms.azurewebsites.net":
                BaseView.WebsiteName = "TexEcon";
                BaseView.WebsiteStyle = "TexEcon";
                BaseView.WebsiteId = 2;
                BaseView.SiteUrl = new Uri("https://mwhcms.azurewebsites.net");
                break;
            case "localhost":
                BaseView.WebsiteName = "MoM";
                BaseView.WebsiteStyle = "MOM";
                BaseView.WebsiteId = 3;
                BaseView.SiteUrl = new Uri("http://mechanicsofmotherhood.com");
                break;

            case "mechanicsofmotherhood.com":
                BaseView.WebsiteName = "MoM";
                BaseView.WebsiteStyle = "MOM";
                BaseView.WebsiteId = 3;
                BaseView.SiteUrl = new Uri("https://mechanicsofmotherhood.com");
                break;

            case "texecon.com":
                BaseView.WebsiteName = "TexEcon";
                BaseView.WebsiteStyle = "TexEcon";
                BaseView.WebsiteId = 2;
                BaseView.SiteUrl = new Uri("https://texecon.com");
                break;
            case "mytest.com":
                BaseView.WebsiteName = "TexEcon";
                BaseView.WebsiteStyle = "TexEcon";
                BaseView.WebsiteId = 2;
                BaseView.SiteUrl = new Uri("http://mytest.com");
                break;

            case "texecon.frogsfolly.com":
                BaseView.WebsiteName = "TexEcon";
                BaseView.WebsiteStyle = "TexEcon";
                BaseView.WebsiteId = 2;
                BaseView.SiteUrl = new Uri("https://texecon.frogsfolly.com");
                break;

            default:
                BaseView.WebsiteName = "TexEcon";
                BaseView.WebsiteStyle = "TexEcon";
                BaseView.WebsiteId = 2;
                BaseView.SiteUrl = new Uri("https://texecon.com");
                break;
        }
        if (BaseView.Menu.Count == 0)
        {
            object synclock = new object();

            var AllPages = GetCache(dbContext);

            BaseView.Menu.AddRange(AllPages.Where(w => w.DomainID == BaseView.WebsiteId).ToArray());
        }
        return BaseView;
    }

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

    /// <summary>
    /// Gets the cache.
    /// </summary>
    /// <returns>IEnumerable&lt;MenuModel&gt;.</returns>
    public static IEnumerable<MenuModel> GetCache(AppDbContext context)
    {
        IEnumerable<MenuModel> myCache = null; // (MenuModel[])HttpRuntime.Cache[AllPageListKey];

        if (myCache == null)
            myCache = Array.Empty<MenuModel>();

        if (!myCache.Any())
        {
            var menuList = new List<MenuModel>();

            //  Get from API
            //
            //
            //BaseViewModel momView = new BaseViewModel();
            //using (var momClient = new MomClient())
            //    {
            //    momView = momClient.GetBaseViewModel();
            //    }
            //menuList.AddRange(momView.Menu);
            //using (var myDB = new DbMenu())
            //    {
            //    var updateMenu = menuList.Where(w => w.Controller.ToLower() != "recipe").ToList();
            //    myDB.Save(updateMenu);
            //    }

            using (var myDB = new MenuProvider(context))
            {
                menuList = myDB.GetAllMenuItems().ToList();
            }
            using (var myDb = new RecipeProvider(context))
            {
                menuList.AddRange(myDb.GetSiteMenu(3));
            }
            myCache = menuList.ToArray();
            SetCache(myCache);
        }
        return myCache;
    }

    /// <summary>
    /// Gets the menu item.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns>MenuModel.</returns>
    public static MenuModel GetMenuItem(this HtmlHelper html)
    {
        if (html == null)
            return new MenuModel();

        var curPath = html.ViewContext.HttpContext.Request.Path.Value
            .ToLower(CultureInfo.CurrentCulture)
            .Split('/');
        if (curPath.Length <= 1)
            return new MenuModel();
        if (curPath.Length == 2)
        {
            var curItem = ((WebsiteVM)html.ViewData.Model).Menu
                .Where(w => w.Action.ToLower(CultureInfo.CurrentCulture) == curPath[1])
                .FirstOrDefault();

            if (curItem != null)
                return curItem;
        }

        if (curPath.Length == 3)
        {
            var curItem = ((WebsiteVM)html.ViewData.Model).Menu
                .Where(w => w.Action.ToLower(CultureInfo.CurrentCulture) == curPath[2])
                .FirstOrDefault();

            if (curItem != null)
                return curItem;
        }
        return new MenuModel();
    }


    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="helper"></param>
    ///// <param name="items"></param>
    ///// <returns></returns>
    //public static HtmlString HorizontalMenu(this HtmlHelper helper, List<MenuModel> items)
    //{
    //    if (items == null || !items.Any())
    //    {
    //        return new HtmlString(String.Empty);
    //    }
    //    var ul = new TagBuilder("ul");
    //    ul.AddCssClass("list-inline list-unstyled");
    //    var sb = new StringBuilder();
    //    items.ForEach(e => CreateMenuItem(e, sb));
    //    //ul.con = sb.ToString();
    //    return new HtmlString(ul.ToString());
    //}

    /// <summary>
    /// IsActive
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="control">The control.</param>
    /// <returns>System.String.</returns>
    public static string IsActive(this HtmlHelper html, string control)
    {
        if (html == null)
            return string.Empty;

        return control == (string)html.ViewContext.RouteData.Values["controller"] ? "active" : string.Empty;
    }

    /// <summary>
    /// IsActive
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="item">The item.</param>
    /// <param name="IsParent">if set to <c>true</c> [is parent].</param>
    /// <returns>System.String.</returns>
    public static string IsActive(this HtmlHelper html, MenuModel item, bool IsParent)
    {
        if (html == null)
            return string.Empty;

        if (item == null)
            return string.Empty;

        var curPath = html.ViewContext.HttpContext.Request.Path.Value
            .ToLower(CultureInfo.CurrentCulture)
            .Split('/');
        if (curPath.Length <= 1)
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
    public static string IsActive(this HtmlHelper html, string control, string action)
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

    /// <summary>
    /// ResetPagesCache
    /// </summary>
    public static void ResetPagesCache()
    {
        //HttpRuntime.Cache.Remove(AllPageListKey);
        //HttpRuntime.Cache[AllPageListKey] = (IEnumerable<MenuModel>)Array.Empty<MenuModel>();
        //HttpContext.Current.Session["ControllerBaseView"] = null;
    }

    /// <summary>
    /// SetCache
    /// </summary>
    /// <param name="pages">The pages.</param>
    public static void SetCache(IEnumerable<MenuModel> pages)
    {
        //HttpRuntime.Cache
        //    .Insert(key: AllPageListKey,
        //            value: pages,
        //            dependencies: null,
        //            absoluteExpiration: Cache.NoAbsoluteExpiration,
        //            slidingExpiration: TimeSpan.FromMinutes(15),
        //            priority: CacheItemPriority.NotRemovable,
        //            onRemoveCallback: null);
    }

    ///// <summary>
    ///// Toolbar
    ///// </summary>
    ///// <param name="helper">The helper.</param>
    ///// <param name="items">The items.</param>
    ///// <returns>HtmlString.</returns>
    //public static HtmlString Toolbar(this HtmlHelper helper, List<MenuModel> items)
    //{
    //    if (items == null || !items.Any())
    //    {
    //        return new HtmlString(String.Empty);
    //    }
    //    var ul = new TagBuilder("ul");
    //    ul.AddCssClass("list-inline list-unstyled toolbar");
    //    var sb = new StringBuilder();
    //    items.ForEach(e => CreateToolbarItem(e, sb));
    //    ul.InnerHtml = sb.ToString();
    //    return new HtmlString(ul.ToString(TagRenderMode.Normal));
    //}

    ///// <summary>
    ///// VerticalMenu
    ///// </summary>
    ///// <param name="helper">The helper.</param>
    ///// <param name="items">The items.</param>
    ///// <returns>HtmlString.</returns>
    //public static HtmlString VerticalMenu(this HtmlHelper helper, List<MenuModel> items)
    //{
    //    if (items == null || !items.Any())
    //    {
    //        return new HtmlString(String.Empty);
    //    }
    //    var ul = new TagBuilder("ul");
    //    ul.AddCssClass("list-unstyled");
    //    var sb = new StringBuilder();
    //    items.ForEach(e => CreateMenuItem(e, sb));
    //    ul.InnerHtml = sb.ToString();
    //    return new HtmlString(ul.ToString(TagRenderMode.Normal));
    //}

    ///// <summary>
    ///// Creates the menu item.
    ///// </summary>
    ///// <param name="menuItem">The menu item.</param>
    ///// <param name="sb">The sb.</param>
    //private static void CreateMenuItem(MenuModel menuItem, StringBuilder sb)
    //{
    //    if (String.IsNullOrEmpty(menuItem.Url))
    //    {
    //        var li = new TagBuilder("li")
    //        { InnerHtml = $"<i class=\"fa fa-{menuItem.Icon}\"></i> {menuItem.Title}" };
    //        sb.Append(li.ToString(TagRenderMode.Normal));
    //    }
    //    else
    //    {
    //        var li = new TagBuilder("li")
    //        {
    //            InnerHtml =
    //            $"<a href=\"{menuItem.Url}\" title=\"{menuItem.Description}\"><i class=\"fa fa-{menuItem.Icon}\"></i> {menuItem.Title}</a>"
    //        };
    //        sb.Append(li.ToString(TagRenderMode.Normal));
    //    }
    //}

    ///// <summary>
    ///// Creates the toolbar item.
    ///// </summary>
    ///// <param name="menuItem">The menu item.</param>
    ///// <param name="sb">The sb.</param>
    //private static void CreateToolbarItem(MenuModel menuItem, StringBuilder sb)
    //{
    //    if (String.IsNullOrEmpty(menuItem.Url))
    //    {
    //        var li = new TagBuilder("li")
    //        { InnerHtml = $"<i title=\"{menuItem.Description}\" class=\"fa fa-{menuItem.Icon}\"></i>" };
    //        sb.Append(li.ToString(TagRenderMode.Normal));
    //    }
    //    else
    //    {
    //        var li = new TagBuilder("li")
    //        {
    //            InnerHtml =
    //            $"<a class=\"btn btn-default btn-sm\" href=\"{menuItem.Url}\" " +
    //                $"title=\"{menuItem.Description}\"><i class=\"fa fa-{menuItem.Icon}\"></i></a>"
    //        };
    //        sb.Append(li.ToString(TagRenderMode.Normal));
    //    }
    //}
}
