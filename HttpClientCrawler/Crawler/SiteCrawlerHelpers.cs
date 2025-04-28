using System.Text;

namespace HttpClientCrawler.Crawler;

/// <summary>
/// Helper methods for the site crawler implementations
/// </summary>
public static class SiteCrawlerHelpers
{
    /// <summary>
    /// Extracts the domain name from a URL, removing the 'www.' prefix if present
    /// </summary>
    /// <param name="url">The URL to extract the domain name from</param>
    /// <returns>The domain name without 'www.' prefix</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is invalid</exception>
    public static string GetDomainName(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        try
        {
            Uri uri = new(url);
            string host = uri.Host;
            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                host = host[4..];
            }
            return host;
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException($"Invalid URL format: {url}", nameof(url), ex);
        }
    }

    /// <summary>
    /// Determines if a URL belongs to the same domain as the request path
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <param name="requestPath">The original request path for comparison</param>
    /// <returns>True if the URL is from the same domain as the request path, otherwise false</returns>
    public static bool IsSameDomain(string url, string requestPath)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(requestPath))
        {
            return false;
        }

        try
        {
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                return false; // Invalid URL, not the same domain
            }

            // Handle protocol-relative URLs (starting with //)
            if (url.StartsWith("//", StringComparison.Ordinal))
            {
                // Replace "//" with "https://" and check if the modified URL is the same domain
                url = "https:" + url;
                return IsSameFullDomain(new Uri(url), requestPath);
            }

            // If the URI is relative, treat it as the same domain
            if (!uri.IsAbsoluteUri)
            {
                return true;
            }

            // Handle HTTP/HTTPS links
            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            {
                return IsSameFullDomain(uri, requestPath);
            }

            // For other schemes (e.g., "ftp", "mailto", etc.), treat them as different domains
            return false;
        }
        catch (Exception)
        {
            return false; // If there's an error, assume not the same domain
        }
    }

    /// <summary>
    /// Checks if a URI belongs to the same domain as the request path
    /// </summary>
    /// <param name="uri">The URI to check</param>
    /// <param name="requestPath">The original request path for comparison</param>
    /// <returns>True if the URI is from the same domain as the request path, otherwise false</returns>
    public static bool IsSameFullDomain(Uri uri, string requestPath)
    {
        if (uri == null || string.IsNullOrWhiteSpace(requestPath))
        {
            return false;
        }

        try
        {
            string host = new Uri(requestPath).Host;
            string targetHost = uri.Host;

            // Check if the target host matches the domain host and the URL has a valid path
            return string.Equals(host, targetHost, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(uri.AbsolutePath);
        }
        catch (Exception)
        {
            return false; // If there's an error, assume not the same domain
        }
    }

    /// <summary>
    /// Determines if a link is valid for crawling based on its extension
    /// </summary>
    /// <param name="link">The link to validate</param>
    /// <returns>True if the link is valid for crawling, otherwise false</returns>
    public static bool IsValidLink(string link)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return false;
        }

        // Check if the link either has no extension or has .html or .htm extension
        string extension = Path.GetExtension(link);
        if (string.IsNullOrEmpty(extension) ||
            extension.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".htm", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".aspx", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".php", StringComparison.OrdinalIgnoreCase))
        {
            // Exclude non-HTML content and system paths
            return !IsExcludedExtension(link) && !IsSystemPath(link);
        }

        return false;
    }

    /// <summary>
    /// Checks if a link has an excluded extension type (media, document, etc.)
    /// </summary>
    /// <param name="link">The link to check</param>
    /// <returns>True if the link has an excluded extension, otherwise false</returns>
    private static bool IsExcludedExtension(string link)
    {
        // List of excluded file extensions
        string[] excludedExtensions = {
            // Images
            ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".svg", ".ico", ".webp",
            // Documents
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".csv", ".rtf", ".txt",
            // Media
            ".mp3", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".wav", ".ogg", ".webm",
            // Archives
            ".zip", ".rar", ".tar", ".gz", ".7z",
            // Other
            ".xml", ".json", ".rss", ".css", ".js", ".woff", ".woff2", ".ttf", ".eot"
        };

        return excludedExtensions.Any(ext => link.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a link points to a system path that should be excluded
    /// </summary>
    /// <param name="link">The link to check</param>
    /// <returns>True if the link points to a system path, otherwise false</returns>
    private static bool IsSystemPath(string link)
    {
        // List of system paths to exclude
        string[] excludedPaths = {
            "/cgi-bin/",
            "/cdn-cgi/",
            "/wp-admin/",
            "/wp-includes/",
            "/wp-content/plugins/",
            "/admin/",
            "/phpmyadmin/"
        };

        return excludedPaths.Any(path => link.Contains(path, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Normalizes a link by removing query parameters and on-page links,
    /// and converting relative links to absolute using the request path as base
    /// </summary>
    /// <param name="link">The link to normalize</param>
    /// <param name="requestPath">The original request path to use as base for relative links</param>
    /// <returns>The normalized link</returns>
    public static string RemoveQueryAndOnPageLinks(string? link, string requestPath)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return string.Empty;
        }

        try
        {
            // Remove query parameters (if any)
            int queryIndex = link.IndexOf('?');
            if (queryIndex >= 0)
            {
                link = link[..queryIndex];
            }

            // Remove on-page links (if any)
            int hashIndex = link.IndexOf('#');
            if (hashIndex >= 0)
            {
                link = link[..hashIndex];
            }

            // Convert relative links to absolute links using the base domain
            if (!link.StartsWith("//", StringComparison.Ordinal) && Uri.TryCreate(link, UriKind.Relative, out var relativeUri))
            {
                Uri baseUri = new(requestPath);
                Uri absoluteUri = new(baseUri, relativeUri);
                link = absoluteUri.ToString();
            }

            return link.ToLowerInvariant();
        }
        catch (Exception)
        {
            return string.Empty; // Return empty string if there's an error
        }
    }

    /// <summary>
    /// Writes a collection of data to a CSV file, excluding specified properties
    /// </summary>
    /// <typeparam name="T">The type of data to write</typeparam>
    /// <param name="data">The collection of data to write</param>
    /// <param name="filePath">The path to the CSV file</param>
    /// <exception cref="ArgumentNullException">Thrown when data or filePath is null</exception>
    /// <exception cref="IOException">Thrown when there's an error writing to the file</exception>
    public static void WriteToCsv<T>(IEnumerable<T> data, string filePath)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        try
        {
            // Ensure directory exists
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using StreamWriter writer = new(filePath, false, Encoding.UTF8);

            // Get properties excluding ResponseResults and RequestBody
            var properties = typeof(T).GetProperties()
                                      .Where(p => p.Name != "ResponseResults" && p.Name != "RequestBody" && p.Name != "ResponseHtmlDocument")
                                      .ToArray();

            // Write CSV header
            writer.WriteLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));

            // Write data rows
            foreach (var item in data)
            {
                var values = new List<string>();
                foreach (var property in properties)
                {
                    if (property.PropertyType.IsGenericType &&
                        property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var listValue = property.GetValue(item) as System.Collections.ICollection;
                        values.Add(listValue != null ? $"\"{listValue.Count}\"" : "\"0\"");
                    }
                    else
                    {
                        var propertyValue = property.GetValue(item);
                        if (propertyValue == null)
                        {
                            values.Add(string.Empty);
                        }
                        else
                        {
                            // Escape quotes and wrap in quotes
                            var valueString = propertyValue.ToString()?.Replace("\"", "\"\"") ?? string.Empty;
                            values.Add($"\"{valueString}\"");
                        }
                    }
                }
                writer.WriteLine(string.Join(",", values));
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Error writing to CSV file: {filePath}", ex);
        }
    }
}
