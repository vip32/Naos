namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static partial class Extensions
    {
        /// <summary>
        /// Adds or updates the entry in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        [DebuggerStepThrough]
        public static void AddOrUpdate<T>(
            this IList<T> source,
            T item)
        {
            if (source == null || item == null)
            {
                return;
            }

            if (source.Contains(item))
            {
                source.Remove(item);
            }

            source.Add(item);
        }

        /// <summary>
        /// Adds or updates the entry in the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        [DebuggerStepThrough]
        public static void AddOrUpdate<T>(
            this ICollection<T> source,
            T item)
        {
            if (source == null || item == null)
            {
                return;
            }

            if (source.Contains(item))
            {
                source.Remove(item);
            }

            source.Add(item);
        }

        /// <summary>
        /// Adds or updates the entry in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        [DebuggerStepThrough]
        public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            TValue value)
        {
            //source ??= new Dictionary<TKey, TValue>();

            if (source == null || key == null)
            {
                return source;
            }

            if (source.ContainsKey(key))
            {
                source.Remove(key);
            }

            source.Add(key, value);

            return source;
        }

        /// <summary>
        /// Adds the given <paramref name="items"/> to the given <paramref name="source"/>.
        /// <remarks>This method is used to duck-type <see cref="IDictionary{TKey, TValue}"/> with multiple pairs.</remarks>
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="items">The items to add.</param>
        [DebuggerStepThrough]
        public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue> items)
        {
            //source ??= new Dictionary<TKey, TValue>();

            if (source == null)
            {
                return source;
            }

            foreach (var item in items.Safe())
            {
                source.AddOrUpdate(item.Key, item.Value);
            }

            return source;
        }
    }
}