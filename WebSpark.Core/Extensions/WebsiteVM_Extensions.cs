using System.Text;
using System.Xml.Linq;
using WebSpark.Core.Models.ViewModels;

namespace WebSpark.Core.Extensions;

/// <summary>
/// Extension methods for generating a sitemap for a WebsiteVM object.
/// </summary>
public static class WebsiteVM_Extensions
{
    /// <summary>
    /// Generates a sitemap.xml file from a list of menu items for a WebsiteVM object.
    /// </summary>
    /// <param name="website">The WebsiteVM object containing the list of menu items.</param>
    /// <returns>An XDocument containing the sitemap.xml file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the website object or its menu list is null.</exception>
    public static XDocument GenerateSitemapXDocument(this WebsiteVM website)
    {
        if (website == null)
        {
            throw new ArgumentNullException(nameof(website), "The website object cannot be null.");
        }

        if (website.Menu == null)
        {
            throw new ArgumentNullException(nameof(website.Menu), "The menu list cannot be null.");
        }

        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(xmlns + "urlset",
                website.Menu.Select(menu =>
                {
                    var url = ReplaceDoubleSlash($"{website.SiteUrl}{menu.Url}");
                    var lastmod = menu.LastModifiedW3C ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:sszzz");
                    return new XElement(xmlns + "url",
                        new XElement(xmlns + "loc", url),
                        new XElement(xmlns + "lastmod", lastmod),
                        new XElement(xmlns + "changefreq", "monthly")
                    );
                })
            )
        );
        return sitemap;
    }

    /// <summary>
    /// Replaces consecutive forward slashes with a single forward slash in a URL string.
    /// </summary>
    /// <param name="input">The URL string to be modified.</param>
    /// <returns>The input URL string with consecutive forward slashes replaced by a single forward slash.</returns>
    public static string ReplaceDoubleSlash(string input)
    {
        int startIndex = input.IndexOf("//") + 2;
        StringBuilder sb = new(input[..startIndex]);

        for (int i = startIndex; i < input.Length; i++)
        {
            if (input[i] == '/' && input[i - 1] == '/')
            {
                continue;
            }
            sb.Append(input[i]);
        }
        return sb.ToString();
    }
}

