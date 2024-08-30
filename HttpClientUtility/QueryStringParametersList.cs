using System.Text;


namespace HttpClientUtility;

/// <summary>
/// Represents a list of query string parameters for an Http request.
/// </summary>
public class QueryStringParametersList
{
    private List<KeyValuePair<string, string>> ParameterList { get; set; } = [];
    /// <summary>
    /// Adds a query string parameter to the list.
    /// </summary>
    /// <param name="key">The key of the parameter.</param>
    /// <param name="value">The value of the parameter.</param>
    public void Add(string? key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
        }

        if (value == null) // Allowing empty value as it might be valid in some cases
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }
        ParameterList.Add(new KeyValuePair<string, string>(key, value));
    }

    /// <summary>
    /// Gets the query string postfix formed by joining all the parameters in the list.
    /// </summary>
    /// <returns>The query string postfix.</returns>
    public string GetQueryStringPostfix()
    {
        var builder = new StringBuilder();
        foreach (var parameter in ParameterList)
        {
            if (builder.Length > 0)
            {
                builder.Append('&');
            }
            builder.Append(Uri.EscapeDataString(parameter.Key))
                   .Append('=')
                   .Append(Uri.EscapeDataString(parameter.Value));
        }
        return builder.ToString();
    }
}
