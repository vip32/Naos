namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        public static string SubstringFrom(this string source, string seperator, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (seperator.IsNullOrEmpty())
            {
                return source;
            }

            return SubstringFromInternal(source, seperator, source.IndexOf(seperator, comparison));
        }

        public static string SubstringFromLast(this string source, string seperator, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (seperator.IsNullOrEmpty())
            {
                return source;
            }

            return SubstringFromInternal(source, seperator, source.LastIndexOf(seperator, comparison));
        }

        private static string SubstringFromInternal(this string source, string seperator, int index)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (seperator.IsNullOrEmpty())
            {
                return source;
            }

            if (index == 0 && index + seperator.Length < source.Length)
            {
                return source.Substring(index + seperator.Length);
            }

            if (index > 0 && index == source.Length)
            {
                return string.Empty;
            }

            if (index > 0 && index + seperator.Length < source.Length)
            {
                return source.Substring(index + seperator.Length);
            }

            return string.Empty;
        }
    }
}
