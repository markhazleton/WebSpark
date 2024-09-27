using System.Text.Json;
namespace WebSpark.Portal.Areas.TriviaSpark.Models.JShow;

public class JShowService(JShowConfig config) : IJShowService
{
    private readonly string _jsonFilePath = config.JsonOutputFolder;
    private JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public List<JShow> GetJShows()
    {
        var jeopardyShows = new List<JShow>();
        var files = Directory.GetFiles(_jsonFilePath, "JSHOW_*.json");

        foreach (var file in files)
        {
            try
            {
                var jShow = ReadJShowFromJson(file);
                if (jShow != null)
                {
                    jeopardyShows.Add(jShow);
                }
            }
            catch (JsonException ex)
            {
                // Log JSON parsing errors and continue processing other files
                Console.WriteLine($"JSON parsing error in file {file}: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log any other errors and continue processing other files
                Console.WriteLine($"Unexpected error in file {file}: {ex.Message}");
            }
        }
        int showNumber = 1;
        foreach (var show in jeopardyShows)
        {
            show.ShowNumber = showNumber;
            showNumber++;
        }
            return jeopardyShows;
    }


    public JShow ReadJShowFromJson(string filePath)
    {
        try
        {
            // Read the JSON file as a string
            string jsonContent = File.ReadAllText(filePath);

            // Deserialize the JSON content into an instance of JShow
            JShow jShow = JsonSerializer.Deserialize<JShow>(jsonContent, _jsonOptions);

            jShow ??= new JShow();

            // Loop over the categories and questions to set the Question Category and Round properties
            jShow.Rounds.DoubleJeopardy.Theme = jShow.Theme;
            jShow.Rounds.Jeopardy.Theme = jShow.Theme;

            foreach (var category in jShow.Rounds.Jeopardy.Categories)
            {
                foreach (var question in category.Questions)
                {
                    question.Theme = jShow.Theme;
                    question.JShowId = jShow.Id;
                    question.Category = category.Name;
                }
            }
            foreach (var category in jShow.Rounds.DoubleJeopardy.Categories)
            {
                foreach (var question in category.Questions)
                {
                    question.Theme = jShow.Theme;
                    question.JShowId = jShow.Id;
                    question.Category = category.Name;
                }
            }
            return jShow;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"File not found: {filePath}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return null;
        }
    }
}
public static class JsonElementExtensions
{
    /// <summary>
    /// Safely gets a property from a JsonElement. Returns null if the property does not exist.
    /// </summary>
    public static JsonElement? GetPropertySafe(this JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Safely converts a JsonElement to a Guid. Returns null if the conversion fails.
    /// </summary>
    public static Guid GetGuid(this JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String && Guid.TryParse(element.GetString(), out var guid))
        {
            return guid;
        }
        return Guid.Empty;
    }
}