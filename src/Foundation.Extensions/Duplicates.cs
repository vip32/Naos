namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        /// <summary>
        /// Selects all duplicate items in the source enumeration. Duplicates are matched based on the selector
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source">the source items</param>
        /// <param name="selector">the selector</param>
        /// <returns></returns>
        public static IEnumerable<TSource> Duplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            var grouped = source.GroupBy(selector); // group by selector
            return grouped.Where(i => IsMultiple(i)).SelectMany(i => i); // select groups with multiple items?

            static bool IsMultiple<T>(IEnumerable<T> source)
            {
                var enumerator = source.GetEnumerator();
                return enumerator.MoveNext() && enumerator.MoveNext();
            }
        }
    }
}
