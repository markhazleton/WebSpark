namespace HttpClientUtility.StringConverter;

/// <summary>
/// Intervece for converting string to model and model to string
/// </summary>
public interface IStringConverter
{
    /// <summary>
    /// Convert string to model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    T ConvertFromString<T>(string value);
    /// <summary>
    /// convert model to string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="model"></param>
    /// <returns></returns>
    string ConvertFromModel<T>(T model);
}