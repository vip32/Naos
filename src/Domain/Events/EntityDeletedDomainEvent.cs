namespace Naos.Core.Domain
{
    public class EntityDeletedDomainEvent<T> : IDomainEvent
        where T : class, IEntity
    {
        public EntityDeletedDomainEvent(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }
}
