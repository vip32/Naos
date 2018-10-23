namespace Naos.Core.Domain
{
    public class EntityDeleteDomainEvent<T> : IDomainEvent
        where T : class, IEntity
    {
        public EntityDeleteDomainEvent(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }
}
