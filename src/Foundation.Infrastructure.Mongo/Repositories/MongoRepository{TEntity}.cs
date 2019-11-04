namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public class MongoRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly bool hasBsonId;

        public MongoRepository(MongoRepositoryOptions<TEntity> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.MongoClient, nameof(options.MongoClient));
            EnsureArg.IsNotNullOrEmpty(options.DatabaseName, nameof(options.DatabaseName));
            EnsureArg.IsNotNullOrEmpty(options.CollectionName, nameof(options.CollectionName));
            EnsureArg.IsNotNull(options.IdGenerator, nameof(options.IdGenerator));

            this.Options = options;
            this.Logger = options.CreateLogger<MongoRepository<TEntity>>();

            this.Collection = options.MongoClient
                .GetDatabase(options.DatabaseName)
                .GetCollection<TEntity>(options.CollectionName);
            this.hasBsonId = Attribute.IsDefined(typeof(TEntity).GetProperty("Id"), typeof(BsonIdAttribute));

            this.Logger.LogInformation($"{{LogKey:l}} construct mongo repository (type={typeof(TEntity).PrettyName()})", LogKeys.DomainRepository);
        }

        public MongoRepository(Builder<MongoRepositoryOptionsBuilder<TEntity>, MongoRepositoryOptions<TEntity>> optionsBuilder)
            : this(optionsBuilder(new MongoRepositoryOptionsBuilder<TEntity>()).Build())
        {
        }

        protected MongoRepositoryOptions<TEntity> Options { get; set; }

        protected ILogger<MongoRepository<TEntity>> Logger { get; }

        protected IMongoCollection<TEntity> Collection { get; }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return (await this.FindOneAsync(id).AnyContext()) != null;
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            if (this.hasBsonId)
            {
                return (await this.Collection.FindAsync(
                    Builders<TEntity>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id as string))).AnyContext()).SingleOrDefault();
            }
            else
            {
                return await this.Collection.Find(e => e.Id.Equals(id)).SingleOrDefaultAsync().AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(Enumerable.Empty<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if (specification == null)
            {
                return await this.FindAllAsync(options, cancellationToken).AnyContext();
            }

            return await this.FindAllAsync(new[] { specification }, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            var expressions = specificationsArray.Safe().Select(s => s.ToExpression());

            return await Task.Run(() =>
            {
                if (options?.HasOrders() == true)
                {
                    return this.Collection.AsQueryable()
                        .WhereExpressions(expressions)
                        .OrderByIf(options)
                        .SkipIf(options?.Skip)
                        .TakeIf(options?.Take)
                        .ToList();
                }
                else
                {
                    return this.Collection.AsQueryable()
                        .WhereExpressions(expressions)
                        .SkipIf(options?.Skip)
                        .TakeIf(options?.Take)
                        .ToList();
                }
            }).AnyContext();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return entity;
            }

            if (entity.Id.IsDefault())
            {
                this.Options.IdGenerator.SetNew(entity);
            }

            await this.Collection.InsertOneAsync(entity).AnyContext();
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                return entity;
            }

            if (this.hasBsonId)
            {
                await this.Collection.ReplaceOneAsync(
                    Builders<TEntity>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(entity.Id as string)), entity).AnyContext();
            }
            else
            {
                await this.Collection.ReplaceOneAsync(e => e.Id == entity.Id, entity).AnyContext();
            }

            return entity;
        }

        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return (null, ActionResult.None);
            }

            var isNew = entity.Id.IsDefault() || !await this.ExistsAsync(entity.Id).AnyContext();

            if (entity.Id.IsDefault())
            {
                this.Options.IdGenerator.SetNew(entity);
            }

            if (this.Options.PublishEvents && this.Options.Mediator != null)
            {
                if (isNew)
                {
                    await this.Options.Mediator.Publish(new EntityInsertDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.Options.Mediator.Publish(new EntityUpdateDomainEvent(entity)).AnyContext();
                }
            }

            this.Logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogKeys.DomainRepository);
            if (isNew)
            {
                if (entity is IStateEntity stateEntity)
                {
                    stateEntity.State.SetCreated();
                }

                entity = await this.InsertAsync(entity).AnyContext();
            }
            else if (entity is IStateEntity stateEntity)
            {
                entity = await this.UpdateAsync(entity).AnyContext();
            }

            if (this.Options.PublishEvents && this.Options.Mediator != null)
            {
                if (isNew)
                {
                    await this.Options.Mediator.Publish(new EntityInsertedDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.Options.Mediator.Publish(new EntityUpdatedDomainEvent(entity)).AnyContext();
                }
            }

            //this.logger.LogInformation($"{{LogKey:l}} upserted entity: {entity.GetType().PrettyName()}, id: {entity.Id}, isNew: {isNew}", LogEventKeys.DomainRepository);
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            return isNew ? (entity, ActionResult.Inserted) : (entity, ActionResult.Updated);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return ActionResult.None;
            }

            DeleteResult result;
            if (this.hasBsonId)
            {
                result = await this.Collection.DeleteOneAsync(
                    Builders<TEntity>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id as string))).AnyContext();
            }
            else
            {
                result = await this.Collection.DeleteOneAsync(e => e.Id == id).AnyContext();
            }

            return result.DeletedCount > 0 ? ActionResult.Deleted : ActionResult.None;
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            if (entity == null)
            {
                return ActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).AnyContext();
        }
    }
}
