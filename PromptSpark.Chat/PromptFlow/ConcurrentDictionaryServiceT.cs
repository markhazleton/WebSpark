using System.Collections.Concurrent;

namespace PromptSpark.Chat.PromptFlow;


public class ConcurrentDictionaryService<T>
{
    private readonly ConcurrentDictionary<string, T> _dictionary = new();

    /// <summary>
    /// Saves or updates a value associated with a key in the dictionary.
    /// </summary>
    /// <param name="key">The key for the item to save or update.</param>
    /// <param name="value">The value to associate with the specified key.</param>
    public void Save(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        _dictionary[key] = value; // Add or update
    }

    /// <summary>
    /// Looks up a value by key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value associated with the key if it exists; otherwise, default value of T.</returns>
    public T? Lookup(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        _dictionary.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// Deletes a value by key.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public bool Delete(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        return _dictionary.TryRemove(key, out _);
    }

    /// <summary>
    /// Checks if the dictionary contains a specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    public bool ContainsKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        return _dictionary.ContainsKey(key);
    }

    /// <summary>
    /// Gets all keys currently stored in the dictionary.
    /// </summary>
    /// <returns>A list of all keys in the dictionary.</returns>
    public IEnumerable<string> GetAllKeys()
    {
        return _dictionary.Keys;
    }

    /// <summary>
    /// Clears all entries in the dictionary.
    /// </summary>
    public void Clear()
    {
        _dictionary.Clear();
    }
}
