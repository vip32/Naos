﻿namespace Naos.Core.Domain
{
    public class EntityUpdatedDomainEvent<T> : IDomainEvent
        where T : class, IEntity
    {
        public EntityUpdatedDomainEvent(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; set; }
    }
}
