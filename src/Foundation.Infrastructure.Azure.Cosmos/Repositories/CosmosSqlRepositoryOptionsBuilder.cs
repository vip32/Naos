namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Naos.Foundation.Domain;

    public class CosmosSqlRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<CosmosSqlRepositoryOptions<TEntity>, CosmosSqlRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public CosmosSqlRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public CosmosSqlRepositoryOptionsBuilder<TEntity> Provider(ICosmosSqlProvider<TEntity> provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public CosmosSqlRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public CosmosSqlRepositoryOptionsBuilder<TEntity> IdGenerator(IEntityIdGenerator<TEntity> idGenerator)
        {
            this.Target.IdGenerator = idGenerator;
            return this;
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination> :
        BaseOptionsBuilder<CosmosSqlRepositoryOptions<TEntity, TDestination>, CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination>>
        where TEntity : class, IEntity, IAggregateRoot
        where TDestination : class, ICosmosEntity
    {
        public CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination> Provider(ICosmosSqlProvider<TDestination> provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination> IdGenerator(IEntityIdGenerator<TEntity> idGenerator)
        {
            this.Target.IdGenerator = idGenerator;
            return this;
        }

        public CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination> Mapper(IEntityMapper mapper)
        {
            this.Target.Mapper = mapper;
            return this;
        }
    }
}