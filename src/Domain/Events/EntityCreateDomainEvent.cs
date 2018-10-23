namespace Naos.Core.Domain
{
    public class EntityCreateDomainEvent<T> : IDomainEvent
        where T : class, IEntity
    {
        public EntityCreateDomainEvent(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class EntityCreateDomainEvent2<T> : MediatR.INotification
#pragma warning restore SA1402 // File may only contain a single class
        where T : class, IEntity
    {
        public EntityCreateDomainEvent2(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }
}
