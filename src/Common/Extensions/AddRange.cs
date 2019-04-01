namespace Naos.Core.Common
{
    using System.Collections.Generic;

    public static partial class Extensions
    {
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            if (source == null || items == null)
            {
                return;
            }

            foreach (T item in items)
            {
                source.Add(item);
            }
        }
    }
}