namespace Naos.Core.Domain
{
    public class EntityDeletedDomainEvent<TEntity, TId> : IDomainEvent
        where TEntity : Entity<TId>, IAggregateRoot
    {
        public EntityDeletedDomainEvent(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; set; }
    }
}
