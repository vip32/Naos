namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        /// <summary>
        /// Adds the item to the result list at the given index (pos).
        /// </summary>
        /// <typeparam name="T">the source.</typeparam>
        /// <param name="source">the source collection.</param>
        /// <param name="item">The item to add to the result.</param>
        /// <param name="index">the index at which the item should inserted.</param>
        public static IEnumerable<T> Insert<T>(
            this IEnumerable<T> source,
            T item,
            int index = 0)
        {
            if (item == null)
            {
                return source;
            }

            if (source == null)
            {
                return new List<T>
                {
                    item
                };
            }

            var result = new List<T>(source);
            if (index >= 0)
            {
                result.Insert(index, item);
            }
            else
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Adds the items to the result list at the given index (pos).
        /// </summary>
        /// <typeparam name="T">the source.</typeparam>
        /// <param name="source">the source collection.</param>
        /// <param name="items">The items to add to the result.</param>
        /// <param name="index">the index at which the item should inserted.</param>
        public static IEnumerable<T> Insert<T>(
            this IEnumerable<T> source,
            IEnumerable<T> items,
            int index = 0)
        {
            foreach (var item in items.Safe().Reverse())
            {
                source = source.Insert(item, index);
            }

            return source;
        }
    }
}
