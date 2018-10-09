namespace Naos.Core.Domain
{
    using System.Threading.Tasks;
    using EnsureThat;

    public static partial class Extensions
    {
        /// <summary>
        /// Finds the entity by identifier and tenant.
        /// </summary>
        /// <typeparam name="TEntity">Type of the result</typeparam>
        /// <typeparam name="TId">The type of the identifier.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns></returns>
        public static async Task<TEntity> FindByIdAsync<TEntity, TId>(
            this IRepository<TEntity, TId> source,
            TId id,
            string tenantId)
            where TEntity : TenantEntity<TId>, IAggregateRoot
            where TId : class
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);

            var entity = await source.FindByIdAsync(id).ConfigureAwait(false);
            if(entity != null && new HasTenantSpecification<TEntity, TId>(tenantId).IsSatisfiedBy(entity))
            {
                return entity;
            }

            return null;
        }
    }
}
