namespace Naos.Core.Common
{
    using System.Collections.Generic;

    public static partial class Extensions
    {
        /// <summary>
        /// Also adds the item to the result list.
        /// </summary>
        /// <typeparam name="TSource">the source</typeparam>
        /// <param name="source">the source collection</param>
        /// <param name="item">The item to add to the result</param>
        /// <param name="index">the index at which the item should inserted</param>
        /// <returns></returns>
        public static IEnumerable<TSource> Insert<TSource>(this IEnumerable<TSource> source, TSource item, int index = 0)
        {
            if(item == null)
            {
                return source;
            }

            if (source == null)
            {
                return new List<TSource>
                {
                    item
                };
            }

            var result = new List<TSource>(source);
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
    }
}
