namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static partial class Extensions
    {
        /// <summary>
        /// Concatenates a specified separator String between each element of a specified enumeration, yielding a single
        /// concatenated string.
        /// </summary>
        /// <typeparam name="T">any object.</typeparam>
        /// <param name="source">The enumeration.</param>
        /// <param name="separator">A String.</param>
        /// <returns>A String consisting of the elements of value interspersed with the separator string.</returns>
        [DebuggerStepThrough]
        public static string ToString<T>(this IEnumerable<T> source, string separator)
        {
            if (source.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return string.Join(separator, source);
        }

        [DebuggerStepThrough]
        public static string ToString<T>(this IEnumerable<T> source, char seperator)
            => ToString(source, seperator.ToString());
    }
}
