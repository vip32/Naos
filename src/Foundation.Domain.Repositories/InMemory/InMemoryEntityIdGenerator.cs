namespace Naos.Foundation.Domain
{
    using System;
    using EnsureThat;

    public class InMemoryEntityIdGenerator<TEntity> : IEntityIdGenerator<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly InMemoryContext<TEntity> context;

        public InMemoryEntityIdGenerator(InMemoryContext<TEntity> context)
        {
            this.context = context;
        }

        public void SetNew(TEntity entity)
        {
            EnsureArg.IsNotNull(entity);

            switch (entity)
            {
                case IEntity<int> e:
                    e.Id = this.context.Entities.Count + 1;
                    break;
                case IEntity<string> e:
                    e.Id = SequentialGuid.NewGuid().ToString();
                    break;
                case IEntity<Guid> e:
                    e.Id = SequentialGuid.NewGuid();
                    break;
                default:
                    throw new NotSupportedException($"entity id type {entity.Id.GetType().Name} not supported");
            }
        }
    }
}
