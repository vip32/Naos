namespace Naos.Core.Domain
{
    public class EntityUpdatedDomainEvent<TEntity> : IDomainEvent
        where TEntity : class, IEntity, IAggregateRoot
    {
        public EntityUpdatedDomainEvent(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; set; }
    }
}
