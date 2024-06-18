
namespace WebSpark.Core.Providers;

public interface IThemeProvider
{
    Task<Dictionary<string, string>> GetSettings(string theme);
}

public class ThemeProvider : IThemeProvider
{
    public async Task<Dictionary<string, string>> GetSettings(string theme)
    {
        var settings = new Dictionary<string, string>
        {
            { "one", "<div>the one</div>" },
            { "two", "<div>the two</div>" }
        };
        return await Task.FromResult(settings);
    }
}
