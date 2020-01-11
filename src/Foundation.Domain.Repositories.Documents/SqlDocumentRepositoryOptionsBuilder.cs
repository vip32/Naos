namespace Naos.Foundation.Domain
{
    using MediatR;

    public class SqlDocumentRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<SqlDocumentRepositoryOptions<TEntity>, SqlDocumentRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public SqlDocumentRepositoryOptionsBuilder<TEntity> Provider(IDocumentProvider<TEntity> provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public SqlDocumentRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public SqlDocumentRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public SqlDocumentRepositoryOptionsBuilder<TEntity> IdGenerator(IEntityIdGenerator<TEntity> idGenerator)
        {
            this.Target.IdGenerator = idGenerator;
            return this;
        }
    }
}