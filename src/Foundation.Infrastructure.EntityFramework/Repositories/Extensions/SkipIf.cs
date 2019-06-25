namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IQueryable extensions.
    /// </summary>
    public static partial class Extensions
    {
        public static IQueryable<T> SkipIf<T>(
            this IQueryable<T> source, int? skip)
            => skip.HasValue && skip.Value > 0 ? source.Skip(skip.Value) : source;

        public static IEnumerable<T> SkipIf<T>(
            this IEnumerable<T> source, int? skip)
            => skip.HasValue && skip.Value > 0 ? source.Skip(skip.Value) : source;
    }
}
