using System.Reflection;
using System.Text.Json;

namespace ConsoleApp1.Service;

public static class JsonSchemaGenerator
{
    public static string GenerateJsonSchema<T>(JsonSerializerOptions options)
    {
        return GetSchemaSerialized<T>(options);
    }

    private static string GetSchemaSerialized<T>(JsonSerializerOptions options)
    {
        var schema = new
        {
            type = "object",
            properties = GeneratePropertiesSchema(typeof(T)),
            additionalProperties = false
        };

        return JsonSerializer.Serialize(schema, options);
    }

    private static Dictionary<string, object> GeneratePropertiesSchema(Type type)
    {
        var propertiesSchema = new Dictionary<string, object>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertySchema = GetPropertySchema(property.PropertyType);
            propertiesSchema.Add(property.Name, propertySchema);
        }

        return propertiesSchema;
    }

    private static object GetPropertySchema(Type propertyType)
    {
        if (propertyType == typeof(string))
        {
            return new { type = "string" };
        }
        else if (propertyType == typeof(int) || propertyType == typeof(long))
        {
            return new { type = "integer" };
        }
        else if (propertyType == typeof(double) || propertyType == typeof(float) || propertyType == typeof(decimal))
        {
            return new { type = "number" };
        }
        else if (propertyType == typeof(bool))
        {
            return new { type = "boolean" };
        }
        else if (typeof(IEnumerable<string>).IsAssignableFrom(propertyType))
        {
            return new
            {
                type = "array",
                items = new { type = "string" }
            };
        }
        else if (typeof(IEnumerable<int>).IsAssignableFrom(propertyType) || typeof(IEnumerable<long>).IsAssignableFrom(propertyType))
        {
            return new
            {
                type = "array",
                items = new { type = "integer" }
            };
        }
        else if (typeof(IEnumerable<double>).IsAssignableFrom(propertyType) || typeof(IEnumerable<float>).IsAssignableFrom(propertyType) || typeof(IEnumerable<decimal>).IsAssignableFrom(propertyType))
        {
            return new
            {
                type = "array",
                items = new { type = "number" }
            };
        }
        else if (propertyType.IsClass)
        {
            return new
            {
                type = "object",
                properties = GeneratePropertiesSchema(propertyType)
            };
        }
        else
        {
            return new { type = "string" }; // Default to string if type is unknown
        }
    }

    public static bool ValidateJsonAgainstSchema<T>(string json, JsonSerializerOptions options)
    {
        var schemaJson = GenerateJsonSchema<T>(options);
        var schemaDocument = JsonDocument.Parse(schemaJson);
        var jsonDocument = JsonDocument.Parse(json);

        // Get schema properties
        if (schemaDocument.RootElement.TryGetProperty("properties", out var schemaProperties))
        {
            return ValidateProperties(schemaProperties, jsonDocument.RootElement);
        }

        Console.WriteLine("Invalid schema format.");
        return false;
    }

    private static bool ValidateProperties(JsonElement schemaProperties, JsonElement jsonElement)
    {
        foreach (var schemaProperty in schemaProperties.EnumerateObject())
        {
            if (!jsonElement.TryGetProperty(schemaProperty.Name, out var jsonProperty))
            {
                Console.WriteLine($"Property '{schemaProperty.Name}' is missing in JSON.");
                return false;
            }

            var schemaType = schemaProperty.Value.GetProperty("type").GetString();
            if (!ValidatePropertyType(schemaType, jsonProperty))
            {
                Console.WriteLine($"Property '{schemaProperty.Name}' is of incorrect type. Expected: {schemaType}");
                return false;
            }
        }

        return true;
    }

    private static bool ValidatePropertyType(string schemaType, JsonElement jsonElement)
    {
        return schemaType switch
        {
            "string" => jsonElement.ValueKind == JsonValueKind.String,
            "integer" => jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out _),
            "number" => jsonElement.ValueKind == JsonValueKind.Number,
            "boolean" => jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False,
            "array" => jsonElement.ValueKind == JsonValueKind.Array,
            "object" => jsonElement.ValueKind == JsonValueKind.Object,
            _ => false
        };
    }
}
