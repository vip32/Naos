namespace Naos.Core.Infrastructure.EntityFramework
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        public static IQueryable<T> TakeIf<T>(
            this IQueryable<T> source, int? take)
            => take.HasValue && take.Value > 0 ? source.Take(take.Value) : source;

        public static IEnumerable<T> TakeIf<T>(
            this IEnumerable<T> source, int? take)
            => take.HasValue && take.Value > 0 ? source.Take(take.Value) : source;
    }
}
