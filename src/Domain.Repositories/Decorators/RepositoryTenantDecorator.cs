namespace Naos.Core.Domain.Repositories
{
    using Naos.Core.Domain.Specifications;

    /// <summary>
    /// <para>Decorates an <see cref="Repositories.IGenericRepository{TEntity}"/>.</para>
    /// <para>
    ///    .-----------.
    ///    | Decorator |
    ///    .-----------.        .------------.
    ///          `------------> | decoratee  |
    ///            (forward)    .------------.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IGenericRepository{TEntity}" />
    public class RepositoryTenantDecorator<TEntity> : RepositorySpecificationDecorator<TEntity>
        where TEntity : class, IEntity, ITenantEntity, IAggregateRoot
    {
        public RepositoryTenantDecorator(string tenantId, IGenericRepository<TEntity> decoratee)
            : base(new Specification<TEntity>(t => t.TenantId == tenantId), decoratee)
        {
        }
    }
}
