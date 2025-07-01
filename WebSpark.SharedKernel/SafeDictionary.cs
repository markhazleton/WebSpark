namespace WebSpark.SharedKernel;

/// <summary>
/// Provides a type-safe, null-safe dictionary wrapper with convenience methods for safe access and mutation.
/// </summary>
/// <typeparam name="TKey">The type of the dictionary key.</typeparam>
/// <typeparam name="TValue">The type of the dictionary value.</typeparam>
public sealed class SafeDictionary<TKey, TValue> where TKey : notnull
{
    private Dictionary<TKey, TValue> _dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeDictionary{TKey, TValue}"/> class.
    /// </summary>
    public SafeDictionary()
    {
        _dictionary = new();
    }

    /// <summary>
    /// Returns a list of string representations of all key-value pairs.
    /// </summary>
    /// <returns>A list of key-value pairs as strings.</returns>
    public List<string> GetList()
    {
        var list = new List<string>();
        foreach (var item in _dictionary)
        {
            list.Add($"{item.Key} - {item.Value}");
        }
        return list;
    }

    /// <summary>
    /// Gets the value associated with the specified key, or default if not found.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <returns>The value if found; otherwise, default.</returns>
    public TValue? GetValue(TKey key)
    {
        if (key is null)
            return default;
        _dictionary.TryGetValue(key, out var vOut);
        return vOut;
    }

    /// <summary>
    /// Replaces the entire dictionary with the provided value.
    /// </summary>
    /// <param name="value">The new dictionary value.</param>
    public void SetValue(Dictionary<TKey, TValue> value)
    {
        if (value is null) { return; }
        _dictionary = value;
    }

    /// <summary>
    /// Sets the value for the specified key. Adds if not present, updates if exists.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(TKey key, TValue? value)
    {
        if (key is null || value is null) { return; }
        _dictionary[key] = value;
    }
}
