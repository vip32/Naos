namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        public static string SubstringTill(this string source, string till, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (till.IsNullOrEmpty())
            {
                return source;
            }

            return SubstringTillInternal(source, till, source.IndexOf(till, comparison));
        }

        public static string SubstringTillLast(this string source, string till, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (till.IsNullOrEmpty())
            {
                return source;
            }

            return SubstringTillInternal(source, till, source.LastIndexOf(till, comparison));
        }

        private static string SubstringTillInternal(this string source, string till, int index)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (till.IsNullOrEmpty())
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
