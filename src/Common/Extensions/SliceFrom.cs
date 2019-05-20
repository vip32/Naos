namespace Naos.Core.Common
{
    using System;
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static string SliceFrom(this string source, string from, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(from.IsNullOrEmpty())
            {
                return source;
            }

            return SliceFromInternal(source, from, source.IndexOf(from, comparison));
        }

        [DebuggerStepThrough]
        public static string SliceFromLast(this string source, string from, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(from.IsNullOrEmpty())
            {
                return source;
            }

            return SliceFromInternal(source, from, source.LastIndexOf(from, comparison));
        }

        private static string SliceFromInternal(this string source, string from, int fromIndex)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(from.IsNullOrEmpty())
            {
                return source;
            }

            if(fromIndex == 0 && fromIndex + from.Length < source.Length)
            {
                return source.Substring(fromIndex + from.Length);
            }

            if(fromIndex > 0 && fromIndex == source.Length)
            {
                return string.Empty;
            }

            if(fromIndex > 0 && fromIndex + from.Length < source.Length)
            {
                return source.Substring(fromIndex + from.Length);
            }

            return string.Empty;
        }
    }
}
