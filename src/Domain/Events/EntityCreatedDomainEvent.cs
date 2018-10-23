namespace Naos.Core.Domain
{
    public class EntityCreatedDomainEvent<T> : IDomainEvent
        where T : class, IEntity
    {
        public EntityCreatedDomainEvent(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }
}
