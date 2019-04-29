namespace Naos.Core.Domain.Repositories
{
    using Naos.Core.Domain.Specifications;

    /// <summary>
    /// <para>Decorates an <see cref="Repositories.IRepository{TEntity}"/>.</para>
    /// <para>
    ///    .-----------.
    ///    | Decorator |
    ///    .-----------.        .------------.
    ///          `------------> | decoratee  |
    ///            (forward)    .------------.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IRepository{TEntity}" />
    public class RepositoryTenantDecorator<TEntity> : RepositorySpecificationDecorator<TEntity>
        where TEntity : class, IEntity, ITenantEntity, IAggregateRoot
    {
        public RepositoryTenantDecorator(string tenantId, IRepository<TEntity> decoratee)
            : base(decoratee, new Specification<TEntity>(t => t.TenantId == tenantId))
        {
        }
    }
}
