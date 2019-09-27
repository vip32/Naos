namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IQueryable extensions.
    /// </summary>
    public static partial class Extensions
    {
        public static IQueryable<TEntity> SkipIf<TEntity>(
            this IQueryable<TEntity> source, int? skip)
            => skip.HasValue && skip.Value > 0 ? source.Skip(skip.Value) : source;

        public static IEnumerable<TEntity> SkipIf<TEntity>(
            this IEnumerable<TEntity> source, int? skip)
            => skip.HasValue && skip.Value > 0 ? source.Skip(skip.Value) : source;
    }
}
