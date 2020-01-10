namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Naos.Foundation.Domain;

    public class SqlServerDocumentRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<SqlServerDocumentRepositoryOptions<TEntity>, SqlServerDocumentRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public SqlServerDocumentRepositoryOptionsBuilder<TEntity> Provider(SqlServerDocumentProvider<TEntity> provider)
        {
            this.Target.Provider = provider;
            return this;
        }

        public SqlServerDocumentRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public SqlServerDocumentRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public SqlServerDocumentRepositoryOptionsBuilder<TEntity> IdGenerator(IEntityIdGenerator<TEntity> idGenerator)
        {
            this.Target.IdGenerator = idGenerator;
            return this;
        }
    }
}