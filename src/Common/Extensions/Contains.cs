namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        public static bool Contains(this string source, string value, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return source.IndexOf(value, comp) >= 0;
        }

        public static bool Contains(this IEnumerable<string> source, string value, StringComparison comp = StringComparison.InvariantCultureIgnoreCase)
        {
            if (source.IsNullOrEmpty())
            {
                return false;
            }

            return !string.IsNullOrEmpty(value) && source.Any(x => string.Compare(x, value, comp) == 0);
        }
    }
}
