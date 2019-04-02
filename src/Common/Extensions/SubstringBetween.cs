namespace Naos.Core.Common
{
    using System;

    public static partial class Extensions
    {
        public static string SubstringBetween(this string source, string from, string till, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return SubstringFrom(source, from, comparison)
                .SubstringTill(till, comparison);
        }
    }
}
