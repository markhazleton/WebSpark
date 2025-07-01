using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace ConsoleApp1.Service;
public class JsonCleaner
{
    /// <summary>
    /// Cleans a given JSON string by removing unnecessary escape characters 
    /// and ensuring valid JSON format.
    /// </summary>
    /// <param name="input">The raw JSON string.</param>
    /// <returns>A cleaned JSON string.</returns>
    public string CleanJsonString(string input)
    {
        // Step 1: Replace `\\r\\n` and `\\n` with actual newlines, or remove them if they are not needed
        string cleanedInput = input.Replace("\\r\\n", string.Empty)
                                   .Replace("\\n", string.Empty);

        // Step 2: Remove unnecessary backslashes before quotes
        cleanedInput = Regex.Replace(cleanedInput, @"\\(?=[""{}:\[\],])", string.Empty);

        // Step 3: Convert double backslashes `\\` to single backslashes `\` if any exist for path formatting etc.
        cleanedInput = cleanedInput.Replace("\\\\", "\\");

        return cleanedInput;
    }

    /// <summary>
    /// Parses the cleaned JSON string into a JObject.
    /// </summary>
    /// <param name="input">The raw JSON string.</param>
    /// <returns>A JObject if parsing succeeds, otherwise null.</returns>
    public JObject ParseJson(string input)
    {
        var cleanedJson = CleanJsonString(input);

        try
        {
            return JObject.Parse(cleanedJson);
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"Failed to parse JSON: {ex.Message}");
            return null;
        }
    }
}
