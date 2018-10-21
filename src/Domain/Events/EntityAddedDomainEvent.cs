namespace Naos.Core.Domain
{
    public class EntityAddedDomainEvent<TEntity> : IDomainEvent
        where TEntity : class, IEntity, IAggregateRoot
    {
        public EntityAddedDomainEvent(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; set; }
    }
}
