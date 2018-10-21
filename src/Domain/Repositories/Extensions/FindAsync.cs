namespace Naos.Core.Domain
{
    using System.Threading.Tasks;
    using EnsureThat;

    public static partial class Extensions
    {
        /// <summary>
        /// Finds the entity by identifier and tenant.
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns></returns>
        public static async Task<T> FindAsync<T>(
            this IRepository<T> source,
            object id,
            string tenantId)
            where T : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);

            var entity = await source.FindOneAsync(id).ConfigureAwait(false);
            if(entity != null && new HasTenantSpecification<T>(tenantId).IsSatisfiedBy(entity))
            {
                return entity;
            }

            return null;
        }
    }
}
