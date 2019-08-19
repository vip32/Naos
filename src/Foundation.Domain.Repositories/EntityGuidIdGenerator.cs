namespace Naos.Foundation.Domain
{
    using System;

    public class EntityGuidIdGenerator<TEntity> : IEntityIdGenerator<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public string CreateNew(TEntity entity)
        {
            return Guid.NewGuid().ToString();
        }
    }
}
