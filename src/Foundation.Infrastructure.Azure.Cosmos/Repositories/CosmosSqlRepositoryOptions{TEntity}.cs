namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Naos.Foundation.Domain;

    public class CosmosSqlRepositoryOptions<TEntity> : BaseOptions
        where TEntity : class, IEntity, IAggregateRoot
    {
        public IMediator Mediator { get; set; }

        public ICosmosSqlProvider<TEntity> Provider { get; set; }

        public bool PublishEvents { get; set; } = true;

        public IEntityIdGenerator<TEntity> IdGenerator { get; set; } = new EntityGuidIdGenerator<TEntity>();
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class CosmosSqlRepositoryOptions<TEntity, TDestination> : BaseOptions
        where TEntity : class, IEntity, IAggregateRoot
        where TDestination : class, ICosmosEntity
    {
        public IMediator Mediator { get; set; }

        public ICosmosSqlProvider<TDestination> Provider { get; set; }

        public bool PublishEvents { get; set; } = true;

        public IEntityIdGenerator<TEntity> IdGenerator { get; set; } = new EntityGuidIdGenerator<TEntity>();

        public IEntityMapper Mapper { get; set; }
    }
}