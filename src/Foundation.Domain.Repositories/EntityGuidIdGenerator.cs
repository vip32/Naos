namespace Naos.Foundation.Domain
{
    using System;
    using EnsureThat;

    public class EntityGuidIdGenerator<TEntity> : IEntityIdGenerator<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public void SetNew(TEntity entity)
        {
            EnsureArg.IsNotNull(entity);

            entity.Id = Guid.NewGuid().ToString();
        }
    }
}
