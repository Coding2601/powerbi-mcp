using System.Collections.Concurrent;

namespace PowerBI_MCP.Utils.Cache
{
    /// <summary>
    /// Generic static cache handler using a thread-safe concurrent dictionary.
    /// </summary>
    /// <typeparam name="TKey">Type of the cache key.</typeparam>
    /// <typeparam name="TValue">Type of the cache value.</typeparam>
    public static class CacheHandler<TKey, TValue>
        where TKey : notnull
    {
        // Internal thread-safe cache storage.
        private static readonly ConcurrentDictionary<TKey, TValue> _cache = new();

        /// <summary>
        /// Adds or updates a value in the cache for the specified key.
        /// </summary>
        /// <param name="key">The key to store the value under.</param>
        /// <param name="value">The value to store.</param>
        public static void Set(TKey key, TValue value)
        { 
            _cache[key] = value; 
        }

        /// <summary>
        /// Retrieves a value from the cache by key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The cached value, or null if not found.</returns>
        public static TValue? Get(TKey key)
        {
            _cache.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Removes a value from the cache by key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the item was removed; otherwise, false.</returns>
        public static bool Remove(TKey key)
        {
            return _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// Clears all items from the cache.
        /// </summary>
        public static void Clear()
        {
            _cache.Clear();
        }
    }
}