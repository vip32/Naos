namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;

    public static partial class Extensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            var keys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (keys.Add(selector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
