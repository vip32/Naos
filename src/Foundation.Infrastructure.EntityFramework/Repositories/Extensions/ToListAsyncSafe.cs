namespace Naos.Foundation.Infrastructure
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
        public static Task<List<TEntity>> ToListAsyncSafe<TEntity>(
            this IQueryable<TEntity> source,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            if (!(source is IAsyncEnumerable<TEntity>))
            {
                return Task.FromResult(source.ToList());
            }

            return source.ToListAsync(cancellationToken);
        }
    }
}
