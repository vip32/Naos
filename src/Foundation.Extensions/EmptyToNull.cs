namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static IEnumerable<T> EmptyToNull<T>(this IEnumerable<T> source)
        {
            if (source.IsNullOrEmpty())
            {
#pragma warning disable S1168 // Empty arrays and collections should be returned instead of null
                return null;
#pragma warning restore S1168 // Empty arrays and collections should be returned instead of null
            }

            return source;
        }

        [DebuggerStepThrough]
        public static string EmptyToNull(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            return source;
        }

        [DebuggerStepThrough]
        public static string Default(this string source, string defaultValue)
        {
            if (string.IsNullOrEmpty(source))
            {
                return defaultValue;
            }

            return source;
        }
    }
}
