namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    public static partial class Extensions
    {
        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">Type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>( // causes issues when used in mapped repos <TEntity, TDesitination>, Unable to cast object of type 'xxxDto' to type 'Naos.Core.Domain.ITenantEntity'. better use the tenant decorator for this
            this IGenericRepository<TEntity> source,
            string tenantId,
            IFindOptions<TEntity> options = null)
            where TEntity : class, IEntity, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);

            return await source.FindAllAsync(
                HasTenantSpecification<TEntity>.Factory.Create(tenantId),
                options).AnyContext();
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">Type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="specification">The specification.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(
            this IGenericRepository<TEntity> source,
            string tenantId,
            Specification<TEntity> specification,
            IFindOptions<TEntity> options = null)
            where TEntity : class, IEntity, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);
            EnsureArg.IsNotNull(specification);

            return await source.FindAllAsync(
                new List<ISpecification<TEntity>>
                {
                    specification,
                    HasTenantSpecification<TEntity>.Factory.Create(tenantId)
                },
                options).AnyContext();
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">Type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(
            this IGenericRepository<TEntity> source,
            string tenantId,
            IEnumerable<Specification<TEntity>> specifications,
            IFindOptions<TEntity> options = null)
            where TEntity : class, IEntity, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);
            var specificationsArray = specifications as Specification<TEntity>[] ?? specifications.ToArray();
            EnsureArg.IsNotNull(specificationsArray);
            EnsureArg.IsTrue(specificationsArray.Any());

            return await source.FindAllAsync(
                new List<Specification<TEntity>>(specificationsArray)
                {
                    HasTenantSpecification<TEntity>.Factory.Create(tenantId)
                },
                options).AnyContext();
        }
    }
}
