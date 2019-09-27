namespace Naos.Foundation.Infrastructure
{
    using System;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public class MongoDbRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<MongoDbRepositoryOptions<TEntity>, MongoDbRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public MongoDbRepositoryOptionsBuilder<TEntity> Setup(IServiceProvider sp, MongoConfiguration configuration = null)
        {
            this.LoggerFactory(sp.GetRequiredService<ILoggerFactory>());
            this.Mediator(sp.GetRequiredService<IMediator>());
            this.MongoClient(sp.GetRequiredService<IMongoClient>());
            this.DatabaseName(configuration.DatabaseName);

            return this;
        }

        public MongoDbRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public MongoDbRepositoryOptionsBuilder<TEntity> MongoClient(IMongoClient client)
        {
            this.Target.MongoClient = client;
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