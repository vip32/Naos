namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using System.Text;

    public static partial class Extensions
    {
        /// <summary>
        /// Concatenates a specified separator String between each element of a specified enumeration, yielding a single
        /// concatenated string.
        /// </summary>
        /// <typeparam name="T">any object</typeparam>
        /// <param name="source">The enumeration</param>
        /// <param name="separator">A String</param>
        /// <returns>A String consisting of the elements of value interspersed with the separator string.</returns>
        public static string ToString<T>(this IEnumerable<T> source, string separator)
        {
            if (source.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var obj in source)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }

                sb.Append(obj);
            }

            return sb.ToString();
        }
    }
}
