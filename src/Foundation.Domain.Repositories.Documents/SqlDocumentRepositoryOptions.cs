namespace Naos.Foundation.Domain
{
    using MediatR;

    public class SqlDocumentRepositoryOptions<TEntity> : BaseOptions
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

        public IDocumentProvider<TEntity> Provider { get; set; }
    }
}
