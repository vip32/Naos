namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static partial class Extensions
    {
        /// <summary>
        /// Adds or updates the entry in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            TValue value)
        {
            source ??= new Dictionary<TKey, TValue>();

            if(key == default)
            {
                return source;
            }

            if(source.ContainsKey(key))
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
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue> items)
        {
            source ??= new Dictionary<TKey, TValue>();

            foreach(var item in items.Safe())
            {
                if(source.ContainsKey(item.Key))
                {
                    source.Remove(item.Key);
                }

                source.Add(item.Key, item.Value);
            }

            return source;
        }
    }
}