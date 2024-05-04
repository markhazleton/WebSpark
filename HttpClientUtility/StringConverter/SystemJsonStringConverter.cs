using System;
using System.Text.Json;

namespace HttpClientUtility.StringConverter;

/// <summary>
/// Converts objects to and from JSON strings using the System.Text.Json library.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SystemJsonStringConverter"/> class.
/// </remarks>
/// <param name="options">The JSON serializer options.</param>
public class SystemJsonStringConverter(JsonSerializerOptions? options = null) : IStringConverter
{
    private readonly JsonSerializerOptions _options = options ?? new JsonSerializerOptions();

    /// <summary>
    /// Converts the specified model object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the model object.</typeparam>
    /// <param name="model">The model object to convert.</param>
    /// <returns>The JSON string representation of the model object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the serialization fails.</exception>
    public string ConvertFromModel<T>(T model)
    {
        try
        {
            return JsonSerializer.Serialize(model, _options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to serialize object of type '{typeof(T)}'.", ex);
        }
    }

    /// <summary>
    /// Converts the specified JSON string to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to convert to.</typeparam>
    /// <param name="value">The JSON string to convert.</param>
    /// <returns>The object of the specified type.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the deserialization fails or the result is null.</exception>
    public T ConvertFromString<T>(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

        try
        {
            var result = JsonSerializer.Deserialize<T>(value, _options);
            return result == null ? throw new InvalidOperationException($"Deserialization of '{typeof(T)}' resulted in null.") : result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize object of type '{typeof(T)}'.", ex);
        }
    }
}
