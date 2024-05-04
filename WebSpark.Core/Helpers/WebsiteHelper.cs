using WebSpark.Core.Infrastructure.BaseClasses;
using WebSpark.Domain.Entities;

namespace WebSpark.Core.Helpers;

public static class WebsiteHelper
{
    public static Menu GetMenuPage(
        string Title,
        string Content,
        FontAwesomeIcons icon = FontAwesomeIcons.home,
        Menu parent = null,
        string controllerName = "Page",
        string actionName = "index",
        string argumentName = null)
    {
        return new Menu()
        {
            DisplayOrder = 10,
            Title = Title,
            Description = Title,
            Controller = controllerName ?? "Page",
            Action = controllerName == "Page" ? "index" : actionName,
            Argument = controllerName == "Page" ? parent == null ? $"{Title.ToLower().Replace(" ", "-")}" : $"{parent.Title.ToLower().Replace(" ", "-")}/{Title.ToLower().Replace(" ", "-")}" : argumentName,
            Icon = $"{icon.Description()}",
            Url = parent == null ? $"{Title.ToLower().Replace(" ", "-")}" : $"{parent.Title.ToLower().Replace(" ", "-")}/{Title.ToLower().Replace(" ", "-")}",
            PageContent = Content ?? string.Empty,
            Parent = parent
        };
    }

}
