namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;

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

        public CosmosDbSqlRepositoryOptionsBuilder<TEntity> Pub(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }
    }
}