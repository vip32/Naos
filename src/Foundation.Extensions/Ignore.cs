namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static IEnumerable<string> Ignore(
            this IEnumerable<string> source,
            IEnumerable<string> values,
            StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            return source.Safe().Where(s => !s.EqualsAny(values));
        }

        [DebuggerStepThrough]
        public static IEnumerable<int> Ignore(
            this IEnumerable<int> source,
            IEnumerable<int> values)
        {
            return source.Safe().Where(s => !s.EqualsAny(values));
        }

        [DebuggerStepThrough]
        public static IEnumerable<long> Ignore(
            this IEnumerable<long> source,
            IEnumerable<long> values)
        {
            return source.Safe().Where(s => !s.EqualsAny(values));
        }
    }
}
