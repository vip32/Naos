namespace Naos.Core.Infrastructure.SqlServer
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        public static IQueryable<TSource> TakeIf<TSource>(
            this IQueryable<TSource> source, int maxItemCount)
            => maxItemCount > 0 ? source.Take(maxItemCount) : source;

        public static IEnumerable<TSource> TakeIf<TSource>(
            this IEnumerable<TSource> source, int maxItemCount)
            => maxItemCount > 0 ? source.Take(maxItemCount) : source;
    }
}
