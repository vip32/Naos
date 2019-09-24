namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using MongoDB.Driver;

    public class MongoDbRepositoryOptionsBuilder :
        BaseOptionsBuilder<MongoDbRepositoryOptions, MongoDbRepositoryOptionsBuilder>
    {
        public MongoDbRepositoryOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder Pub(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder Client(IMongoClient client)
        {
            this.Target.Client = client;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder Database(string database)
        {
            this.Target.Database = database;
            return this;
        }
    }
}