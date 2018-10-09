namespace Naos.Core.Domain
{
    public class EntityAddedDomainEvent<TEntity, TId> : IDomainEvent
        where TEntity : Entity<TId>, IAggregateRoot
    {
        public EntityAddedDomainEvent(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; set; }
    }
}
