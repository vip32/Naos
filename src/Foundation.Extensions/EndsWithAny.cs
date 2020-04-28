namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;

    public static partial class Extensions
    {
        public static bool EndsWithAny(
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

                if (source.EndsWith(item, comp))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
