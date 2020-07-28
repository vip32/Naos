namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static partial class Extensions
    {
        /// <summary>
        /// Safely removes the item from the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        [DebuggerStepThrough]
        public static bool SafeRemove<T>(
            this IList<T> source,
            T item)
        {
            if (source.IsNullOrEmpty() || item == null)
            {
                return false;
            }

            if (source.Contains(item))
            {
                return source.Remove(item);
            }

            return false;
        }

        /// <summary>
        /// Safely removes the item from the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        [DebuggerStepThrough]
        public static bool SafeRemove<T>(
            this ICollection<T> source,
            T item)
        {
            if (source.IsNullOrEmpty() || item == null)
            {
                return false;
            }

            if (source.Contains(item))
            {
                return source.Remove(item);
            }

            return false;
        }

        /// <summary>
        /// Adds or updates the entry in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        [DebuggerStepThrough]
        public static IDictionary<TKey, TValue> SafeRemove<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key)
        {
            if (source == null || key == null)
            {
                return source;
            }

            if (source.ContainsKey(key))
            {
                source.Remove(key);
            }

            return source;
        }
    }
}