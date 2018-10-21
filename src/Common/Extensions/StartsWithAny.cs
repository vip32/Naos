namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;

    public static partial class Extensions
    {
        public static bool StartsWithAny(
            this string source,
            IEnumerable<string> items,
            StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            if (items.IsNullOrEmpty())
            {
                return false;
            }

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                if (source.StartsWith(item, comp))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
