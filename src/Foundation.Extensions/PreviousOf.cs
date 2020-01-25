namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        public static T PreviousOf<T>(this List<T> source, T item)
        {
            if (source.IsNullOrEmpty())
            {
                return default;
            }

            if (source.FirstOrDefault().Equals(item)) // stop at first item
            {
                return default;
            }

            return source.TakeWhile(x => !x.Equals(item)).DefaultIfEmpty(source[source.Count - 1]).LastOrDefault();
        }

        public static T PreviousOf<TKey, T>(this IDictionary<TKey, T> source, T item)
        {
            if (source.IsNullOrEmpty())
            {
                return default;
            }

            if (item.IsDefault())
            {
                return source.LastOrDefault().Value;
            }

            if (source.FirstOrDefault().Value.Equals(item)) // stop at first item
            {
                return default;
            }

            return source.TakeWhile(x => !x.Value.Equals(item)).LastOrDefault().Value;
        }
    }
}
