namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IQueryable extensions.
    /// </summary>
    public static partial class Extensions
    {
        public static IQueryable<TEntity> TakeIf<TEntity>(
            this IQueryable<TEntity> source, int? take)
            => take.HasValue && take.Value > 0 ? source.Take(take.Value) : source;

        public static IEnumerable<TEntity> TakeIf<TEntity>(
            this IEnumerable<TEntity> source, int? take)
            => take.HasValue && take.Value > 0 ? source.Take(take.Value) : source;
    }
}
