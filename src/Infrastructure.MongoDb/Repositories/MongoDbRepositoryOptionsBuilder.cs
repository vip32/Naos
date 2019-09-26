namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public class MongoDbRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<MongoDbRepositoryOptions<TEntity>, MongoDbRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public MongoDbRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder<TEntity> Pub(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder<TEntity> Client(IMongoClient client)
        {
            this.Target.Client = client;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder<TEntity> DatabaseName(string name)
        {
            this.Target.DatabaseName = name;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder<TEntity> CollectionName(string name)
        {
            this.Target.CollectionName = name;
            return this;
        }
    }
}