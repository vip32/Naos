namespace Naos.Core.Common
{
    using System.Collections.Generic;

    public static partial class Extensions
    {
        /// <summary>
        /// Adds or updates the entry in the dictionary.
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IDictionary<string, T> AddOrUpdate<T>(this IDictionary<string, T> source, string key, T value)
        {
            source = source ?? new Dictionary<string, T>();
            if (key.IsNullOrEmpty())
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
    }
}