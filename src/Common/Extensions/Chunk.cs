namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        /// <summary>
        /// Split sequence in batches of specified size
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="source">Enumeration source</param>
        /// <param name="size">Size of the batch chunk</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int size)
        {
            if (source.IsNullOrEmpty())
            {
                yield return null;
            }

            while (source.Any())
            {
                yield return source.Take(size);
                source = source.Skip(size);
            }
        }
    }
}