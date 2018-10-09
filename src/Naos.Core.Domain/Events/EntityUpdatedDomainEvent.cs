namespace Naos.Core.Domain
{
    public class EntityUpdatedDomainEvent<TEntity, TId> : IDomainEvent
        where TEntity : Entity<TId>, IAggregateRoot
    {
        public EntityUpdatedDomainEvent(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; set; }
    }
}
