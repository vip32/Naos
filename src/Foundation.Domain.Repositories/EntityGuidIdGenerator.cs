namespace Naos.Foundation.Domain
{
    using System;
    using EnsureThat;

    public class EntityGuidIdGenerator<TEntity> : IEntityIdGenerator<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly bool sequential;

        public EntityGuidIdGenerator(bool sequential = true)
        {
            this.sequential = sequential;
        }

        public void SetNew(TEntity entity)
        {
            EnsureArg.IsNotNull(entity);

            switch (entity)
            {
                case IEntity<string> e:
                    e.Id = this.sequential
                        ? SequentialGuid.NewGuid().ToString()
                        : Guid.NewGuid().ToString();
                    break;
                case IEntity<Guid> e:
                    e.Id = this.sequential
                        ? SequentialGuid.NewGuid()
                        : Guid.NewGuid();
                    break;
                default:
                    throw new NotSupportedException($"entity id type {entity.Id.GetType().Name} not supported");
            }
        }
    }
}
