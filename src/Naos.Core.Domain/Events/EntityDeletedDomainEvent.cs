namespace Naos.Core.Domain
{
    public class EntityDeletedDomainEvent<TEntity> : IDomainEvent
        where TEntity : class, IEntity, IAggregateRoot
    {
        public EntityDeletedDomainEvent(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; set; }
    }
}
