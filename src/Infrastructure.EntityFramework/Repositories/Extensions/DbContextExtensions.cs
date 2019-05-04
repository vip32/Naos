namespace Naos.Core.Infrastructure.EntityFramework
{
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.EntityFrameworkCore;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    /// <summary>
    /// DbContext extensions.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// <para>
        /// Saves the changes only for the TEntity type (Aggregate). All other types
        /// are ignored and not saved.
        /// </para>
        /// <para>
        /// Preventing SaveChanges for other entity types can also be prevented by using
        /// seperate DbContext instances.
        /// </para>
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to save.</typeparam>
        /// <param name="source">The source.</param>
        public static async Task<int> SaveChangesAsync<TEntity>(this DbContext source)
            where TEntity : class, IEntity, IAggregateRoot
        {
            EnsureArg.IsNotNull(source, nameof(source));

            var other = source.ChangeTracker.Entries()
                .Where(x => !typeof(TEntity).IsAssignableFrom(x.Entity.GetType()) && x.State != EntityState.Unchanged)
                .GroupBy(x => x.State)
                .ToList();

            foreach(var entry in source.ChangeTracker.Entries()
                .Where(x => !typeof(TEntity).IsAssignableFrom(x.Entity.GetType())))
            {
                entry.State = EntityState.Unchanged;
            }

            var result = await source.SaveChangesAsync().AnyContext();

            foreach(var state in other)
            {
                foreach(var entry in state)
                {
                    entry.State = state.Key;
                }
            }

            return result;
        }
    }
}
