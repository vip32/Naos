﻿namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        /// <summary>
        /// Performs an action on each value of the enumerable.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="source">The items.</param>
        /// <param name="action">Action to perform on every item.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if(source == null)
            {
                return null;
            }

            var retVal = source as T[] ?? source.ToArray();
            foreach(var value in retVal)
            {
                if(action != null && !value.IsDefault())
                {
                    action(value);
                }
            }

            return retVal;
        }
    }
}
