namespace Naos.Core.Domain.Repositories
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    public static partial class Extensions
    {
        /// <summary>
        /// Finds the entity by identifier and tenant.
        /// </summary>
        /// <typeparam name="TEntity">Type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns></returns>
        public static async Task<TEntity> FindOneAsync<TEntity>(
            this IRepository<TEntity> source,
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
