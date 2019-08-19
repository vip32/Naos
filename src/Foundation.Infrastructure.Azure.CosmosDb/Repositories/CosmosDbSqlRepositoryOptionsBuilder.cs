namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Naos.Foundation.Domain;

    public class CosmosDbSqlRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<CosmosDbSqlRepositoryOptions<TEntity>, CosmosDbSqlRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public CosmosDbSqlRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public CosmosDbSqlRepositoryOptionsBuilder<TEntity> Provider(ICosmosDbSqlProvider<TEntity> provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public CosmosDbSqlRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public CosmosDbSqlRepositoryOptionsBuilder<TEntity> IdGenerator(IEntityIdGenerator<TEntity> idGenerator)
        {
            this.Target.IdGenerator = idGenerator;
            return this;
        }
    }
}