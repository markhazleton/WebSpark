namespace TriviaSpark.Core.Utility
{
    /// <summary>
    /// Provides a list-based implementation for managing objects of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to manage.</typeparam>
    public class ListProvider<T> where T : class, IComparable<T>
    {
        public ListProvider()
        {

        }
        public ListProvider(IEnumerable<T> values)
        {
            Add(values);
        }
        public IEnumerable<T> Items => _list ?? Enumerable.Empty<T>();
        private List<T>? _list;

        /// <summary>
        /// Adds an object to the managed list, only if it is not already present.
        /// </summary>
        /// <param name="item">The object to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the item parameter is null.</exception>
        public int Add(T item)
        {
            if (item is null) return 0;

            _list ??= [];

            if (_list.Contains(item))
            {
                return 0;
            }
            _list.Add(item);
            return 1;
        }
        public int Add(IEnumerable<T> items)
        {
            int itemsAdded = 0;
            if (items is null)
            {
                return itemsAdded;
            }

            foreach (var question in items)
            {
                if (question == null)
                {
                    continue;
                }
                if (Add(question) == 1) itemsAdded++;
            }
            return itemsAdded;
        }

        /// <summary>
        /// Removes all objects from the managed list.
        /// </summary>
        public void Clear()
        {
            _list?.Clear();
        }

        /// <summary>
        /// Gets a sequence of objects from the managed list, up to the specified count.
        /// </summary>
        /// <param name="count">The maximum number of objects to retrieve.</param>
        /// <returns>A sequence of objects from the managed list.</returns>
        public IEnumerable<T> Get(int count)
        {
            if (_list is not null)
            {
                return _list.Take(count);
            }
            return Enumerable.Empty<T>();
        }
        public IEnumerable<T> Get(IEnumerable<T>? list, int count)
        {
            if (_list is null)
            {
                return Enumerable.Empty<T>();
            }
            return list is null ? Get(count) : _list.Except(list).Take(count);
        }
        /// <summary>
        /// Returns a random member from the managed list.
        /// </summary>
        /// <returns>A random member from the managed list, or null if the list is empty.</returns>
        public T? GetRandom()
        {
            if (_list is null || _list.Count == 0)
            {
                return null;
            }

            int randomIndex = new Random().Next(0, _list.Count);
            return _list[randomIndex];
        }


        /// <summary>
        /// Removes an object from the managed list.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        /// <returns>True if the object was removed; false if it was not found in the list.</returns>
        public bool Remove(T item)
        {
            if (_list is null)
            {
                return false;
            }
            else if (_list.Contains(item))
            {
                return _list.Remove(item);
            }
            return false;
        }


        public int Count
        {
            get
            {
                return _list?.Count ?? 0;
            }
        }

        public T? First
        {
            get
            {
                return _list?.FirstOrDefault();
            }
        }
    }
}
