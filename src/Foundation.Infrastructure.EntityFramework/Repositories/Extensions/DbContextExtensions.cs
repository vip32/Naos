namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using EnsureThat;
    using Microsoft.EntityFrameworkCore;
    using Naos.Foundation.Domain;

    /// <summary>
    /// DbContext extensions.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// <para>
        /// Saves the changes only for the TEntity type (Aggregate). All other aggregate instances
        /// are ignored and not saved.
        /// </para>
        /// <para>
        /// Preventing SaveChanges for other aggregates can also be prevented by using
        /// seperate DbContext instances.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the entity to save.</typeparam>
        /// <param name="source">The source.</param>
        public static async Task<int> SaveChangesAsync<T>(this DbContext source)
            where T : class//, IEntity, IAggregateRoot
        {
            EnsureArg.IsNotNull(source, nameof(source));

            // find all other aggregates
            var other = source.ChangeTracker.Entries()
                .Where(x => x.Entity.GetType() is IAggregateRoot
                    && !typeof(T).IsAssignableFrom(x.Entity.GetType())
                    && x.State != EntityState.Unchanged)
                .GroupBy(x => x.State)
                .ToList();

            // set all other aggregates to unchanged
            foreach (var entry in source.ChangeTracker.Entries()
                .Where(x => x.Entity.GetType() is IAggregateRoot
                    && !typeof(T).IsAssignableFrom(x.Entity.GetType())))
            {
                // WARN: this is not 100% fool proof: as other modified aggregate child entities are not marked as unchanged
                entry.State = EntityState.Unchanged;
            }

            var result = await source.SaveChangesAsync().AnyContext();

            // set all other aggregates to original state
            foreach (var state in other)
            {
                foreach (var entry in state)
                {
                    entry.State = state.Key;
                }
            }

            return result;
        }

        /// <summary>
        /// Executes the provided operation in a TransactionScope and using the retry strategy currently defined for the dbcontext.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static async Task ExecuteScopedAsync(this DbContext source, Func<Task> operation)
        {
            EnsureArg.IsNotNull(source, nameof(source));
            EnsureArg.IsNotNull(operation, nameof(operation));

            var strategy = source.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = new TransactionScope())
                {
                    await operation().AnyContext();
                    transaction.Complete();
                }
            }).AnyContext();
        }
    }
}
