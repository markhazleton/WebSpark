using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// Provides functionality to parse and respect robots.txt files for web crawlers
/// </summary>
public class RobotsTxtParser
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, List<string>> _disallowedPaths = new();
    private readonly HashSet<string> _processedDomains = new();
    private readonly string _userAgent;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of the RobotsTxtParser
    /// </summary>
    /// <param name="httpClientFactory">Factory to create HTTP clients</param>
    /// <param name="userAgent">User agent string to use when making requests</param>
    /// <param name="logger">Logger for diagnostic information</param>
    public RobotsTxtParser(IHttpClientFactory httpClientFactory, string userAgent, ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("RobotsTxtParser");
        _userAgent = userAgent.ToLowerInvariant();
        _logger = logger;
    }

    /// <summary>
    /// Processes the robots.txt file for the specified domain
    /// </summary>
    /// <param name="url">The URL of the site to process robots.txt for</param>
    /// <param name="cancellationToken">Cancellation token to stop the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task ProcessRobotsTxtAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(url);
            string domain = uri.Host;

            if (_processedDomains.Contains(domain))
            {
                return;
            }

            string robotsUrl = $"{uri.Scheme}://{domain}/robots.txt";
            var response = await _httpClient.GetAsync(robotsUrl, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                ParseRobotsTxt(domain, content);
            }

            _processedDomains.Add(domain);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing robots.txt for {Url}", url);
        }
    }

    /// <summary>
    /// Parses the content of a robots.txt file and extracts disallowed paths
    /// </summary>
    /// <param name="domain">The domain the robots.txt belongs to</param>
    /// <param name="content">The content of the robots.txt file</param>
    private void ParseRobotsTxt(string domain, string content)
    {
        var disallowedPaths = new List<string>();
        string? currentUserAgent = null;
        bool isRelevantUserAgent = false;

        using (StringReader reader = new(content))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    continue;
                }

                // Parse user-agent line
                if (line.StartsWith("User-agent:", StringComparison.OrdinalIgnoreCase))
                {
                    // New user agent section, save previous one if it's relevant
                    if (isRelevantUserAgent && currentUserAgent != null)
                    {
                        _disallowedPaths[domain] = new List<string>(disallowedPaths);
                    }

                    // Start a new user agent section
                    currentUserAgent = line.Substring("User-agent:".Length).Trim().ToLowerInvariant();
                    isRelevantUserAgent = currentUserAgent == "*" || currentUserAgent == _userAgent;

                    if (isRelevantUserAgent)
                    {
                        disallowedPaths = new List<string>();
                    }
                }
                // Parse disallow line if we're in a relevant user agent section
                else if (isRelevantUserAgent && line.StartsWith("Disallow:", StringComparison.OrdinalIgnoreCase))
                {
                    string path = line.Substring("Disallow:".Length).Trim();
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        disallowedPaths.Add(path);
                    }
                }
            }

            // Save the last user agent section if it's relevant
            if (isRelevantUserAgent && currentUserAgent != null && disallowedPaths.Count > 0)
            {
                _disallowedPaths[domain] = new List<string>(disallowedPaths);
            }
        }
    }

    /// <summary>
    /// Checks if a URL is allowed to be crawled according to the robots.txt rules
    /// </summary>
    /// <param name="url">The URL to check against robots.txt rules</param>
    /// <returns>True if the URL is allowed to be crawled, false otherwise</returns>
    public bool IsAllowed(string url)
    {
        try
        {
            var uri = new Uri(url);
            string domain = uri.Host;
            string path = uri.AbsolutePath;

            if (!_processedDomains.Contains(domain) || !_disallowedPaths.ContainsKey(domain))
            {
                return true;
            }

            foreach (var disallowedPath in _disallowedPaths[domain])
            {
                if (disallowedPath.EndsWith('*'))
                {
                    // Handle wildcard at the end
                    string prefix = disallowedPath.TrimEnd('*');
                    if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                else if (disallowedPath.StartsWith('*'))
                {
                    // Handle wildcard at the beginning
                    string suffix = disallowedPath.TrimStart('*');
                    if (path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                else if (disallowedPath.Contains('*'))
                {
                    // Handle wildcards in the middle
                    string pattern = "^" + Regex.Escape(disallowedPath).Replace("\\*", ".*") + "$";
                    if (Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase))
                    {
                        return false;
                    }
                }
                else if (path.StartsWith(disallowedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking robots.txt permissions for {Url}", url);
            return true; // If there's an error, we'll allow crawling
        }
    }
}