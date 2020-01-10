namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Naos.Foundation.Domain;

    public class SqlServerDocumentRepositoryOptions<TEntity> : BaseOptions
        where TEntity : class, IEntity, IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the mediator.
        /// </summary>
        /// <value>
        /// The mediator.
        /// </value>
        public IMediator Mediator { get; set; }

        public bool PublishEvents { get; set; } = true; // Obsolete > optional decorator

        public IEntityIdGenerator<TEntity> IdGenerator { get; set; } = new EntityGuidIdGenerator<TEntity>();

        public SqlServerDocumentProvider<TEntity> Provider { get; set; }
    }
}
