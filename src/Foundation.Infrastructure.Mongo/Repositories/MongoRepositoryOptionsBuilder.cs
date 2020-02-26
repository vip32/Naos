namespace Naos.Foundation.Infrastructure
{
    using System;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public class MongoRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<MongoRepositoryOptions<TEntity>, MongoRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public MongoRepositoryOptionsBuilder<TEntity> Setup(IServiceProvider sp, MongoConfiguration configuration = null)
        {
            this.LoggerFactory(sp.GetRequiredService<ILoggerFactory>());
            this.Mediator(sp.GetRequiredService<IMediator>());
            this.MongoClient(sp.GetRequiredService<IMongoClient>());
            this.DatabaseName(configuration.DatabaseName);

            return this;
        }

        public MongoRepositoryOptionsBuilder<TEntity> Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public MongoRepositoryOptionsBuilder<TEntity> PublishEvents(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public MongoRepositoryOptionsBuilder<TEntity> MongoClient(IMongoClient client)
        {
            this.Target.MongoClient = client;
            return this;
        }

        public MongoRepositoryOptionsBuilder<TEntity> DatabaseName(string name)
        {
            this.Target.DatabaseName = name;
            return this;
        }

        public MongoRepositoryOptionsBuilder<TEntity> CollectionName(string name)
        {
            this.Target.CollectionName = name;
            return this;
        }

        public MongoRepositoryOptionsBuilder<TEntity> Mapper(IEntityMapper mapper)
        {
            this.Target.Mapper = mapper;
            return this;
        }

        public MongoRepositoryOptionsBuilder<TEntity> IdGenerator(IEntityIdGenerator<TEntity> idGenerator)
        {
            this.Target.IdGenerator = idGenerator;
            return this;
        }
    }
}