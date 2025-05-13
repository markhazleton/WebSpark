using System.Text.Json.Serialization;

namespace WebSpark.Portal.Areas.GitHubSpark.Models;

/// <summary>
/// Represents search-related data from GitHub GraphQL API
/// </summary>
public class GraphQLSearchData
{
    [JsonPropertyName("search")]
    public GraphQLSearchResult? Search { get; set; }
}

/// <summary>
/// Represents a search result from GitHub GraphQL API
/// </summary>
public class GraphQLSearchResult
{
    [JsonPropertyName("userCount")]
    public int UserCount { get; set; }

    [JsonPropertyName("edges")]
    public List<GraphQLSearchEdge>? Edges { get; set; } = new();
}

/// <summary>
/// Represents an edge in a search result from GitHub GraphQL API
/// </summary>
public class GraphQLSearchEdge
{
    [JsonPropertyName("node")]
    public GraphQLSearchNode? Node { get; set; }
}

/// <summary>
/// Represents a node in a search result from GitHub GraphQL API
/// </summary>
public class GraphQLSearchNode
{
    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("repositories")]
    public GraphQLSearchRepositories? Repositories { get; set; }
}

/// <summary>
/// Represents repositories information in a search result from GitHub GraphQL API
/// </summary>
public class GraphQLSearchRepositories
{
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}