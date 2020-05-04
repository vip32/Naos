namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> source) // TODO: or SafeAny()?
        {
            return source == null || !source.Any();
        }

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty<TSource>(this ICollection<TSource> source) // TODO: or SafeAny()?
        {
            return source == null || !source.Any();
        }

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this Stream source)
        {
            return source == null || source.Length == 0;
        }

        //public static bool IsNullOrEmpty<TSource>(this IReadOnlyCollection<TSource> source)
        //{
        //    return source == null || !source.Any();
        //}

        public static bool IsNullOrEmpty(this Guid value)
        {
            return IsNullOrEmptyInternal(value);
        }

        public static bool IsNullOrEmpty(this Guid? value)
        {
            return IsNullOrEmptyInternal(value);
        }

        private static bool IsNullOrEmptyInternal(this Guid? value)
        {
            if (value == null)
            {
                return true;
            }

            if (value == default || value == Guid.Empty)
            {
                return true;
            }

            if (string.IsNullOrEmpty(value.ToString()))
            {
                return true;
            }

            return false;
        }
    }
}
