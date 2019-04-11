namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            if(source == null || items == null)
            {
                return;
            }

            foreach(var item in items)
            {
                source.Add(item);
            }
        }

        [DebuggerStepThrough]
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> items)
        {
            if(source == null || items == null)
            {
                return;
            }

            foreach(var pair in items)
            {
                source.Add(pair);
            }
        }
    }
}