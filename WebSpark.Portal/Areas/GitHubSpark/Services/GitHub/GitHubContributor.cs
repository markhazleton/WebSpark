using System.Text.Json.Serialization;
namespace WebSpark.Portal.Areas.GitHubSpark.Services.GitHub;


public class GitHubContributor
{
    public GitHubContributor()
    {
        Login = string.Empty;
        NodeId = string.Empty;
        AvatarUrl = string.Empty;
        GravatarId = string.Empty;
        Url = string.Empty;
        HtmlUrl = string.Empty;
        FollowersUrl = string.Empty;
        FollowingUrl = string.Empty;
        GistsUrl = string.Empty;
        StarredUrl = string.Empty;
        SubscriptionsUrl = string.Empty;
        OrganizationsUrl = string.Empty;
        ReposUrl = string.Empty;
        EventsUrl = string.Empty;
        ReceivedEventsUrl = string.Empty;
        Type = string.Empty;
        UserViewType = string.Empty;
    }

    [JsonPropertyName("login")]
    public string Login { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("gravatar_id")]
    public string GravatarId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }

    [JsonPropertyName("followers_url")]
    public string FollowersUrl { get; set; }

    [JsonPropertyName("following_url")]
    public string FollowingUrl { get; set; }

    [JsonPropertyName("gists_url")]
    public string GistsUrl { get; set; }

    [JsonPropertyName("starred_url")]
    public string StarredUrl { get; set; }

    [JsonPropertyName("subscriptions_url")]
    public string SubscriptionsUrl { get; set; }

    [JsonPropertyName("organizations_url")]
    public string OrganizationsUrl { get; set; }

    [JsonPropertyName("repos_url")]
    public string ReposUrl { get; set; }

    [JsonPropertyName("events_url")]
    public string EventsUrl { get; set; }

    [JsonPropertyName("received_events_url")]
    public string ReceivedEventsUrl { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("user_view_type")]
    public string UserViewType { get; set; }

    [JsonPropertyName("site_admin")]
    public bool SiteAdmin { get; set; }

    [JsonPropertyName("contributions")]
    public int Contributions { get; set; }
}
