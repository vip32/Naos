namespace Naos.Foundation.Infrastructure
{
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public static partial class Extensions
    {
        public static IQueryable<TEntity> TrackChangesIf<TEntity>(
            this DbSet<TEntity> source,
            bool trackchanges)
        where TEntity : class
        {
            return trackchanges ? source : source.AsNoTracking();
        }

        public static IQueryable<TDestination> TrackChangesIf<TEntity, TDestination>(
            this DbSet<TDestination> source,
            bool trackchanges)
        where TEntity : class
        where TDestination : class
        {
            return trackchanges ? source : source.AsNoTracking();
        }
    }
}
