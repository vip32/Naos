namespace Naos.Core.Domain.Repositories
{
    using Naos.Core.Domain.Specifications;

    public class RepositoryTenantDecorator<TEntity> : RepositorySpecificationDecorator<TEntity>
        where TEntity : class, IEntity, ITenantEntity, IAggregateRoot
    {
        public RepositoryTenantDecorator(string tenantId, IRepository<TEntity> decoratee)
            : base(decoratee, new Specification<TEntity>(t => t.TenantId == tenantId))
        {
        }
    }
}
