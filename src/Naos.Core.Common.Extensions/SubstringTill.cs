namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        public static string SubstringTill(this string source, string seperator, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (seperator.IsNullOrEmpty())
            {
                return source;
            }

            return SubstringTillInternal(source, seperator, source.IndexOf(seperator, comparison));
        }

        public static string SubstringTillLast(this string source, string seperator, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (seperator.IsNullOrEmpty())
            {
                return source;
            }

            return SubstringTillInternal(source, seperator, source.LastIndexOf(seperator, comparison));
        }

        private static string SubstringTillInternal(this string source, string seperator, int index)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (seperator.IsNullOrEmpty())
            {
                return source;
            }

            if (index == 0)
            {
                return string.Empty;
            }

            if (index > 0)
            {
                return source.Substring(0, index);
            }

            return source;
        }
    }
}
