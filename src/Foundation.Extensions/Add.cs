namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Diagnostics;
    aaaaaaaaaaaa
    public static partial class Extensions
    {
        /// <summary>
        /// Adds the <paramref name="items"/> to the <paramref name="source"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        [DebuggerStepThrough]
        public static void Add<T>(this IList<T> source, IEnumerable<T> items)
        {
            if (source == null)
            {
                return;
            }

            foreach (var item in items.Safe())
            {
                source.Add(item);
            }
        }
    }
}