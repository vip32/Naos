namespace Naos.Foundation.Domain
{
    using System.Threading.Tasks;
    using EnsureThat;

    public static partial class Extensions
    {
        /// <summary>
        /// Finds the entity by identifier and tenant.
        /// </summary>
        /// <typeparam name="TEntity">Type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        public static async Task<TEntity> FindOneAsync<TEntity>(
            this IGenericRepository<TEntity> source,
            object id,
            string tenantId)
            where TEntity : class, IEntity, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);

            var entity = await source.FindOneAsync(id).AnyContext();
            if(entity != null && new HasTenantSpecification<TEntity>(tenantId).IsSatisfiedBy(entity))
            {
                return entity;
            }

            return default;
        }
    }
}
