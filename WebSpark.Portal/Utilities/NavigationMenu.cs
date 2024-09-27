using System.Text.Json.Serialization;
namespace WebSpark.Portal.Utilities;


public class MenuItem
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("keywords")]
    public string Keywords { get; set; }
}

public class NavigationMenu
{
    [JsonPropertyName("menuItems")]
    public List<MenuItem> MenuItems { get; set; }
}

