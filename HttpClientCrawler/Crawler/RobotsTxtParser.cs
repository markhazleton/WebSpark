using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// Provides functionality to parse and respect robots.txt files
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
    public RobotsTxtParser(IHttpClientFactory httpClientFactory, string userAgent, ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("RobotsTxtParser");
        _userAgent = userAgent.ToLowerInvariant();
        _logger = logger;
    }

    /// <summary>
    /// Processes the robots.txt file for the specified domain
    /// </summary>
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
            var response = await _httpClient.GetAsync(robotsUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                ParseRobotsTxt(domain, content);
            }

            _processedDomains.Add(domain);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing robots.txt for {Url}", url);
        }
    }

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