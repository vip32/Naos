namespace Naos.Core.Common
{
    using System;
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static string SubstringBetween(this string source, string from, string till, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return SubstringFrom(source, from, comparison)
                .SubstringTill(till, comparison);
        }
    }
}
