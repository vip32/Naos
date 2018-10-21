namespace Naos.Core.Domain
{
    public class EntityUpdatedDomainEvent<T> : IDomainEvent
        where T : class, IEntity, IAggregateRoot
    {
        public EntityUpdatedDomainEvent(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }
}
