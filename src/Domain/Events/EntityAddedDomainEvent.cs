namespace Naos.Core.Domain
{
    public class EntityAddedDomainEvent<T> : IDomainEvent
        where T : class, IEntity, IAggregateRoot
    {
        public EntityAddedDomainEvent(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }
}
