using System.Text.Json.Serialization;

namespace WebSpark.Portal.Areas.GitHubSpark.Models;

/// <summary>
/// Represents a GitHub GraphQL API response
/// </summary>
public class GraphQLResponse
{
    [JsonPropertyName("data")]
    public GraphQLData? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<GraphQLError>? Errors { get; set; } = new();
}

/// <summary>
/// Represents an error in a GitHub GraphQL API response
/// </summary>
public class GraphQLError
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("locations")]
    public List<GraphQLErrorLocation>? Locations { get; set; }

    [JsonPropertyName("path")]
    public List<string>? Path { get; set; }

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}

/// <summary>
/// Represents the location of an error in a GitHub GraphQL API response
/// </summary>
public class GraphQLErrorLocation
{
    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("column")]
    public int Column { get; set; }
}

/// <summary>
/// Represents the data section of a GitHub GraphQL API response
/// </summary>
public class GraphQLData
{
    [JsonPropertyName("user")]
    public GraphQLUser? User { get; set; }

    [JsonPropertyName("search")]
    public GraphQLSearchResult? Search { get; set; }
}

/// <summary>
/// Represents a user in the GitHub GraphQL API
/// </summary>
public class GraphQLUser
{
    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [JsonPropertyName("twitterUsername")]
    public string? TwitterUsername { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("repositories")]
    public GraphQLRepositories? Repositories { get; set; }

    [JsonPropertyName("followers")]
    public GraphQLFollowers? Followers { get; set; }

    [JsonPropertyName("following")]
    public GraphQLFollowers? Following { get; set; }
}

/// <summary>
/// Represents a collection of repositories in the GitHub GraphQL API
/// </summary>
public class GraphQLRepositories
{
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("nodes")]
    public List<GraphQLRepository>? Nodes { get; set; } = new();
}

/// <summary>
/// Represents a repository in the GitHub GraphQL API
/// </summary>
public class GraphQLRepository
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("stargazerCount")]
    public int StargazerCount { get; set; }

    [JsonPropertyName("forkCount")]
    public int ForkCount { get; set; }

    [JsonPropertyName("isPrivate")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("primaryLanguage")]
    public GraphQLLanguage? PrimaryLanguage { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a programming language in the GitHub GraphQL API
/// </summary>
public class GraphQLLanguage
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }
}

/// <summary>
/// Represents a collection of followers in the GitHub GraphQL API
/// </summary>
public class GraphQLFollowers
{
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("nodes")]
    public List<GraphQLFollower>? Nodes { get; set; } = new();
}

/// <summary>
/// Represents a follower in the GitHub GraphQL API
/// </summary>
public class GraphQLFollower
{
    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}