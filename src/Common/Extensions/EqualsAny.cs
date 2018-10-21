namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;

    public static partial class Extensions
    {
        public static bool EqualsAny(
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

                if (source.Equals(item, comp))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsAny(
            this int source,
            IEnumerable<int> items)
        {
            if (items.IsNullOrEmpty())
            {
                return false;
            }

            foreach (var item in items)
            {
                if (source == item)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsAny(
            this long source,
            IEnumerable<long> items)
        {
            if (items.IsNullOrEmpty())
            {
                return false;
            }

            foreach (var item in items)
            {
                if (source == item)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
