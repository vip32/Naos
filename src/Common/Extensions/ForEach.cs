namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static partial class Extensions
    {
        /// <summary>
        /// Performs an action on each value of the enumerable.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="source">The items.</param>
        /// <param name="action">Action to perform on every item.</param>
        /// <returns>the source with the actions applied.</returns>
        [DebuggerStepThrough]
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            var itemsArray = source as T[] ?? source.ToArray();

            foreach(var value in itemsArray)
            {
                if(action != null && !EqualityComparer<T>.Default.Equals(value, default(T)))
                {
                    action(value);
                }
            }

            return itemsArray;
        }

        [DebuggerStepThrough]
        public static ICollection<T> ForEach<T>(this ICollection<T> source, Action<T> action)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            return source.AsEnumerable().ForEach(action).ToList();
        }

        public static IReadOnlyCollection<T> ForEach<T>(this IReadOnlyCollection<T> source, Action<T> action)
        {
            return source.AsEnumerable().ForEach(action).ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> childSelector, Action<T> action = null)
        {
            if(source.IsNullOrEmpty())
            {
                return;
            }

            if(action == null)
            {
                return;
            }

            foreach(var item in source)
            {
                action(item);
                childSelector?.Invoke(item).ForEach(childSelector, action);
            }
        }
    }
}
