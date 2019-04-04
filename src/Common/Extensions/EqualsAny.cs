namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;

    public static partial class Extensions
    {
        public static bool EqualsAny(
            this string source,
            IEnumerable<string> values,
            StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            if(string.IsNullOrEmpty(source))
            {
                return false;
            }

            if(values.IsNullOrEmpty())
            {
                return false;
            }

            foreach(var value in values)
            {
                if(value == null)
                {
                    continue;
                }

                if(source.Equals(value, comp))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsAny(
            this int source,
            IEnumerable<int> values)
        {
            if(values.IsNullOrEmpty())
            {
                return false;
            }

            foreach(var value in values)
            {
                if(source == value)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsAny(
            this long source,
            IEnumerable<long> values)
        {
            if(values.IsNullOrEmpty())
            {
                return false;
            }

            foreach(var value in values)
            {
                if(source == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
