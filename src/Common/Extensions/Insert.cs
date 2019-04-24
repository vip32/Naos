namespace Naos.Core.Common
{
    using System.Collections.Generic;

    public static partial class Extensions
    {
        /// <summary>
        /// Adds the item to the result list atr the given index (pos).
        /// </summary>
        /// <typeparam name="TItem">the source</typeparam>
        /// <param name="source">the source collection</param>
        /// <param name="item">The item to add to the result</param>
        /// <param name="index">the index at which the item should inserted</param>
        /// <returns></returns>
        public static IEnumerable<TItem> Insert<TItem>(
            this IEnumerable<TItem> source,
            TItem item,
            int index = 0)
        {
            if(item == null)
            {
                return source;
            }

            if(source == null)
            {
                return new List<TItem>
                {
                    item
                };
            }

            var result = new List<TItem>(source);
            if(index >= 0)
            {
                result.Insert(index, item);
            }
            else
            {
                result.Add(item);
            }

            return result;
        }
    }
}
