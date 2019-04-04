namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        public static string SubstringFrom(this string source, string from, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(from.IsNullOrEmpty())
            {
                return source;
            }

            return SubstringFromInternal(source, from, source.IndexOf(from, comparison));
        }

        public static string SubstringFromLast(this string source, string from, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(from.IsNullOrEmpty())
            {
                return source;
            }

            return SubstringFromInternal(source, from, source.LastIndexOf(from, comparison));
        }

        private static string SubstringFromInternal(this string source, string from, int index)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(from.IsNullOrEmpty())
            {
                return source;
            }

            if(index == 0 && index + from.Length < source.Length)
            {
                return source.Substring(index + from.Length);
            }

            if(index > 0 && index == source.Length)
            {
                return string.Empty;
            }

            if(index > 0 && index + from.Length < source.Length)
            {
                return source.Substring(index + from.Length);
            }

            return string.Empty;
        }
    }
}
