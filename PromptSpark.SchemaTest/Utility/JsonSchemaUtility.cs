using Newtonsoft.Json.Schema.Generation;

namespace PromptSpark.SchemaTest.Utility;

public class JsonSchemaUtility
{
    /// <summary>
    /// Generic function that generates a JSON schema for the given type T.
    /// </summary>
    /// <typeparam name="T">The type for which to generate the JSON schema.</typeparam>
    /// <returns>A JSON schema string representing the given type T.</returns>
    public string GenerateJsonSchema<T>()
    {
        var generator = new JSchemaGenerator();
        var schema = generator.Generate(typeof(T));
        return schema.ToString();

    }
}
