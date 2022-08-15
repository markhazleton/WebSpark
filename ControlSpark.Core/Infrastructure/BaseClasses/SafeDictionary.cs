
namespace ControlSpark.Core.Infrastructure.BaseClasses;

/// <summary>
/// Safe Dictionary
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public sealed class SafeDictionary<TKey, TValue>
{
    private Dictionary<TKey, TValue> _Dictionary;

    /// <summary>
    ///
    /// </summary>
    public SafeDictionary() { _Dictionary = new Dictionary<TKey, TValue>(); }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public List<string> GetList()
    {
        var list = new List<string>();
        if (_Dictionary is not null)
        {
            foreach (var item in _Dictionary)
            {
                list.Add($"{item.Key} - {item.Value}");
            }
        }
        return list;
    }

    /// <summary>
    /// Get Value
    /// </summary>
    /// <param name="key"></param>
    public TValue? GetValue(TKey? key)
    {
        if (_Dictionary is null)
        {
            return default;
        }

        if (key is not null)
        {
            _Dictionary.TryGetValue(key, out var vOut);
            if (vOut == null)
            {
                return default;
            }
            else
            {
                return vOut;
            }
        }
        return default;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    public void SetValue(Dictionary<TKey, TValue> value)
    {
        if (_Dictionary is null) { return; }
        if (value is null) { return; }

        _Dictionary = value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(TKey? key, TValue? value)
    {
        if (_Dictionary is null) { return; }
        if (key is null) { return; }
        if (value is null) { return; }

        if (key is not null)
        {
            _Dictionary.TryGetValue(key, out var vOut);
            if (vOut == null)
            {
                _Dictionary.Add(key, value);
            }
            else
            {
                _Dictionary[key] = value;
            }
        }
    }
}
