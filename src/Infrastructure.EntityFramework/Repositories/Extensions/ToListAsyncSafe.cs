namespace Naos.Core.Infrastructure.EntityFramework
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// IQueryable extensions.
    /// </summary>
    public static partial class Extensions
    {
        public static Task<List<TSource>> ToListAsyncSafe<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            if(source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            if(!(source is IAsyncEnumerable<TSource>))
            {
                return Task.FromResult(source.ToList());
            }

            return source.ToListAsync(cancellationToken);
        }
    }
}
