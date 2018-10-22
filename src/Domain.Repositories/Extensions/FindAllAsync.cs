namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Domain.Specifications;

    public static partial class Extensions
    {
        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> FindAllAsync<T>(
            this IRepository<T> source,
            string tenantId,
            IFindOptions<T> options = null)
            where T : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);

            return await source.FindAllAsync(
                HasTenantSpecification<T>.Factory.Create(tenantId),
                options).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="specification">The specification.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> FindAllAsync<T>(
            this IRepository<T> source,
            string tenantId,
            Specification<T> specification,
            IFindOptions<T> options = null)
            where T : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);
            EnsureArg.IsNotNull(specification);

            return await source.FindAllAsync(
                new List<Specification<T>>
                {
                    specification,
                    HasTenantSpecification<T>.Factory.Create(tenantId)
                },
                options).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds all asynchronous.
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="specifications">The specifications.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> FindAllAsync<T>(
            this IRepository<T> source,
            string tenantId,
            IEnumerable<Specification<T>> specifications,
            IFindOptions<T> options = null)
            where T : class, ITenantEntity, IAggregateRoot
        {
            EnsureArg.IsNotNullOrEmpty(tenantId);
            var specificationsArray = specifications as Specification<T>[] ?? specifications.ToArray();
            EnsureArg.IsNotNull(specificationsArray);
            EnsureArg.IsTrue(specificationsArray.Any());

            return await source.FindAllAsync(
                new List<Specification<T>>(specificationsArray)
                {
                    HasTenantSpecification<T>.Factory.Create(tenantId)
                },
                options).ConfigureAwait(false);
        }
    }
}
