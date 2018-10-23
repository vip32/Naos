namespace Naos.Core.Domain
{
    public class EntityUpdateDomainEvent<T> : IDomainEvent
        where T : class, IEntity
    {
        public EntityUpdateDomainEvent(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }
}
