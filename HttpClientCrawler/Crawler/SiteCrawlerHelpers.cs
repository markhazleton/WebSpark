using System.Collections;
using System.Text;

namespace HttpClientCrawler.Crawler;

public static class SiteCrawlerHelpers
{
    public static string GetDomainName(string url)
    {
        Uri uri = new(url);
        string host = uri.Host;
        if (host.StartsWith("www."))
        {
            host = host[4..];
        }
        return host;
    }
    public static bool IsSameDomain(string url, string RequestPath)
    {
        if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return false; // Invalid URL, not the same _domain
        }

        if (url.StartsWith("//"))
        {
            // Replace "//" with "https://" and check if the modified URL is the same domain
            url = "https:" + url;
            return IsSameFullDomain(new Uri(url), RequestPath);
        }

        // If the URI is relative, treat it as the same domain
        if (!uri.IsAbsoluteUri)
        {
            return true;
        }

        // Handle partial scheme links
        if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        {
            return IsSameFullDomain(uri, RequestPath);
        }
        else if (url.StartsWith("//"))
        {
            // Replace "//" with "https://" and check if the modified URL is the same domain
            url = "https:" + url;
            return IsSameFullDomain(new Uri(url), RequestPath);
        }

        // For other schemes (e.g., "ftp", "mailto", etc.), treat them as different domains
        return false;
    }

    public static bool IsSameFullDomain(Uri uri, string RequestPath)
    {
        string host = new Uri(RequestPath).Host;
        string targetHost = uri.Host;

        // Check if the target host matches the _domain host and the URL has a valid path
        return string.Equals(host, targetHost, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(uri.AbsolutePath)
            && uri.AbsolutePath != "/";
    }

    public static bool IsValidLink(string link)
    {
        // Check if the link either has no extension or has .html or .htm extension
        string extension = Path.GetExtension(link);
        if (string.IsNullOrEmpty(extension) || extension.Equals(".html", StringComparison.OrdinalIgnoreCase) || extension.Equals(".htm", StringComparison.OrdinalIgnoreCase))
        {
            // Exclude image, XML, and video links
            return !link.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                && !link.EndsWith(".mov", StringComparison.OrdinalIgnoreCase)
                && !link.Contains("/cgi-bin/")
                && !link.Contains("/cdn-cgi/");
        }
        return false;
    }

    public static string RemoveQueryAndOnPageLinks(string? link, string RequestPath)
    {
        if (string.IsNullOrWhiteSpace(link)) return string.Empty;

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
        if (!link.StartsWith("//") && Uri.TryCreate(link, UriKind.Relative, out var relativeUri))
        {
            Uri baseUri = new(RequestPath);
            Uri absoluteUri = new(baseUri, relativeUri);
            link = absoluteUri.ToString();
        }
        return link.ToLower();
    }
    public static void WriteToCsv<T>(IEnumerable<T> data, string filePath)
    {
        using StreamWriter writer = new(filePath, false, Encoding.UTF8);
        // Get properties excluding ResponseResults and RequestBody
        var properties = typeof(T).GetProperties()
                                  .Where(p => p.Name != "ResponseResults" && p.Name != "RequestBody" && p.Name != "ResponseHtmlDocument")
                                  .ToArray();

        // Write CSV header
        writer.WriteLine(string.Join(",", properties.Select(p => p.Name)));

        // Write data rows
        foreach (var item in data)
        {
            var values = new List<string>();
            foreach (var property in properties)
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listValue = (ICollection)property.GetValue(item);
                    values.Add(listValue.Count.ToString());
                }
                else
                {
                    var propertyValue = property.GetValue(item);
                    var valueString = propertyValue != null ? "\"" + propertyValue.ToString().Replace("\"", "\"\"") + "\"" : string.Empty; // Handling comma and quotes in data
                    values.Add(valueString);
                }
            }
            writer.WriteLine(string.Join(",", values));
        }
    }

}
