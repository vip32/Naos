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
}