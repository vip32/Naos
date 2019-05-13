namespace Naos.Core.Domain.Repositories
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class InMemoryRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<InMemoryRepositoryOptions<TEntity>, InMemoryRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public InMemoryRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public InMemoryRepositoryOptionsBuilder<TEntity> Provider(ICosmosDbSqlProvider<TEntity> provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public InMemoryRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }
    }
}