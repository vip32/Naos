namespace Naos.Foundation.Infrastructure
{
    using Humanizer;
    using MediatR;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public class MongoDbRepositoryOptions<TEntity> : BaseOptions
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

        public IMongoClient MongoClient { get; set; }

        public string DatabaseName { get; set; } = "master";

        public string CollectionName { get; set; } = typeof(TEntity).Name.Pluralize();

        public IEntityIdGenerator<TEntity> IdGenerator { get; set; } = new EntityGuidIdGenerator<TEntity>();
    }
}