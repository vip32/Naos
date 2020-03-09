namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        public static T NextOf<T>(this List<T> source, T item)
        {
            if (source.IsNullOrEmpty())
            {
                return default;
            }

            if (source.LastOrDefault().Equals(item)) // stop at last item
            {
                return default;
            }

            return source.SkipWhile(x => !x.Equals(item)).Skip(1).DefaultIfEmpty(source[0]).FirstOrDefault();
        }

        public static T NextOf<TKey, T>(this IDictionary<TKey, T> source, T item)
        {
            if (source.IsNullOrEmpty())
            {
                return default;
            }

            if (item.IsDefault())
            {
                return source.FirstOrDefault().Value;
            }

            if (source.LastOrDefault().Value.Equals(item)) // stop at last item
            {
                return default;
            }

            return source.SkipWhile(x => !x.Value.Equals(item)).Skip(1).FirstOrDefault().Value;
        }
    }
}
