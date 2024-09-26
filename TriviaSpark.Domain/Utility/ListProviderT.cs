namespace TriviaSpark.Domain.Utility;

/// <summary>
/// Represents a provider of items.
/// </summary>
/// <typeparam name="T">The type of items.</typeparam>
public abstract class ListProvider<T> where T : class, IComparable<T>
{
    private readonly ReaderWriterLockSlim _lock = new(); // For thread safety
    private HashSet<T>? _hashSet; // Optimized storage for unique elements and faster lookups
    private List<T>? _list; // To maintain the order of elements

    public ListProvider()
    {
        _hashSet = [];
        _list = [];
    }

    public ListProvider(IEnumerable<T> values) : this()
    {
        Add(values);
    }

    public IEnumerable<T> Items
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _list?.ToList() ?? Enumerable.Empty<T>(); // Return a copy to ensure thread safety
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _list?.Count ?? 0;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public T? First
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _list?.FirstOrDefault();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public int Add(T item)
    {
        if (item is null) return 0;

        _lock.EnterWriteLock();
        try
        {
            if (_hashSet == null || _list == null)
            {
                _hashSet = [];
                _list = [];
            }

            if (_hashSet.Contains(item))
            {
                return 0; // Item already exists
            }

            _hashSet.Add(item);
            _list.Add(item);
            return 1;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public int Add(IEnumerable<T> items)
    {
        if (items is null) return 0;

        int itemsAdded = 0;

        _lock.EnterWriteLock();
        try
        {
            foreach (var item in items)
            {
                if (item != null && _hashSet != null && _list != null && !_hashSet.Contains(item))
                {
                    _hashSet.Add(item);
                    _list.Add(item);
                    itemsAdded++;
                }
            }
            return itemsAdded;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Remove(T item)
    {
        if (item is null) return false;

        _lock.EnterWriteLock();
        try
        {
            if (_hashSet == null || _list == null || !_hashSet.Contains(item)) return false;

            _hashSet.Remove(item);
            _list.Remove(item);
            return true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet?.Clear();
            _list?.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IEnumerable<T> Get(int count)
    {
        _lock.EnterReadLock();
        try
        {
            return _list?.Take(count).ToList() ?? Enumerable.Empty<T>(); // Return a copy to ensure thread safety
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerable<T> Get(IEnumerable<T>? list, int count)
    {
        _lock.EnterReadLock();
        try
        {
            if (_list == null) return Enumerable.Empty<T>();

            var excludedList = list?.ToList() ?? [];
            return _list.Except(excludedList).Take(count).ToList(); // Return a copy to ensure thread safety
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public T? GetRandom()
    {
        _lock.EnterReadLock();
        try
        {
            if (_list == null || _list.Count == 0) return null;

            int randomIndex = new Random().Next(0, _list.Count);
            return _list[randomIndex];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // Additional methods:

    /// <summary>
    /// Finds the first object that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match the object.</param>
    /// <returns>The first matching object, or null if no match is found.</returns>
    public T? Find(Predicate<T> predicate)
    {
        _lock.EnterReadLock();
        try
        {
            return _list?.Find(predicate);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Checks if an object is present in the managed list.
    /// </summary>
    /// <param name="item">The object to check for.</param>
    /// <returns>True if the object is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        _lock.EnterReadLock();
        try
        {
            return _hashSet?.Contains(item) ?? false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Sorts the managed list.
    /// </summary>
    public void Sort()
    {
        _lock.EnterWriteLock();
        try
        {
            _list?.Sort();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}