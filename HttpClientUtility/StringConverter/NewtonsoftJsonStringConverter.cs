using Newtonsoft.Json;
using System;

namespace HttpClientUtility.StringConverter;

/// <summary>
/// Converts strings to and from JSON using Newtonsoft.Json library.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NewtonsoftJsonStringConverter"/> class.
/// </remarks>
/// <param name="settings">The JSON serializer settings.</param>
public class NewtonsoftJsonStringConverter(JsonSerializerSettings? settings = null) : IStringConverter
{
    private readonly JsonSerializerSettings _settings = settings ?? new JsonSerializerSettings();

    /// <summary>
    /// Converts a string to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the string to.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The converted object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the value is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization fails or the result is null.</exception>
    public T ConvertFromString<T>(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

        try
        {
            var result = JsonConvert.DeserializeObject<T>(value, _settings);
            return result == null ? throw new InvalidOperationException($"Deserialization of '{typeof(T)}' resulted in null.") : result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize object of type '{typeof(T)}'.", ex);
        }
    }

    /// <summary>
    /// Converts the specified model object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the model object.</typeparam>
    /// <param name="model">The model object to convert.</param>
    /// <returns>The JSON string representation of the model object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when serialization fails or the result is null.</exception>
    public string ConvertFromModel<T>(T model)
    {
        try
        {
            var result = JsonConvert.SerializeObject(model, _settings);
            return result ?? throw new InvalidOperationException($"Serialization of '{typeof(T)}' resulted in null.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to serialize object of type '{typeof(T)}'.", ex);
        }
    }
}
