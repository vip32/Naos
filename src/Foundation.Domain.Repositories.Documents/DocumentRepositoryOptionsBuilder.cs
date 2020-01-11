namespace Naos.Foundation.Domain
{
    using MediatR;

    public class DocumentRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<DocumentRepositoryOptions<TEntity>, DocumentRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public DocumentRepositoryOptionsBuilder<TEntity> Provider(IDocumentProvider<TEntity> provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public DocumentRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public DocumentRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public DocumentRepositoryOptionsBuilder<TEntity> IdGenerator(IEntityIdGenerator<TEntity> idGenerator)
        {
            this.Target.IdGenerator = idGenerator;
            return this;
        }
    }
}