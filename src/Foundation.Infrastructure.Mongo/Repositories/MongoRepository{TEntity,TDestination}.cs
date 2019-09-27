namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    //using MongoDB.Bson;
    //using MongoDB.Bson.Serialization;
    //using MongoDB.Bson.Serialization.IdGenerators;
    //using MongoDB.Bson.Serialization.Serializers;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public class MongoRepository<TEntity, TDestination> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
        where TDestination : class, IMongoEntity
    {
        private readonly ILogger<IGenericRepository<TEntity>> logger;

        public MongoRepository(
            MongoRepositoryOptions<TEntity> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.MongoClient, nameof(options.MongoClient));
            EnsureArg.IsNotNullOrEmpty(options.DatabaseName, nameof(options.DatabaseName));
            EnsureArg.IsNotNullOrEmpty(options.CollectionName, nameof(options.CollectionName));
            EnsureArg.IsNotNull(options.IdGenerator, nameof(options.IdGenerator));
            EnsureArg.IsNotNull(options.Mapper, nameof(options.Mapper));

            this.Options = options;
            this.logger = options.CreateLogger<IGenericRepository<TEntity>>();

            this.Collection = options.MongoClient
                .GetDatabase(options.DatabaseName)
                .GetCollection<TDestination>(options.CollectionName);
        }

        public MongoRepository(Builder<MongoRepositoryOptionsBuilder<TEntity>, MongoRepositoryOptions<TEntity>> optionsBuilder)
            : this(optionsBuilder(new MongoRepositoryOptionsBuilder<TEntity>()).Build())
        {
        }

        protected MongoRepositoryOptions<TEntity> Options { get; set; }

        protected IMongoCollection<TDestination> Collection { get; }

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

            var result = await this.Collection.Find(e => e.Id.Equals(id)).SingleOrDefaultAsync().AnyContext();
            return this.Options.Mapper.Map<TEntity>(result);
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
            var expressions = specificationsArray.Safe().Select(s => this.EnsurePredicate(s));

            var result = await Task.Run(() =>
            {
                if (options?.HasOrders() == true)
                {
                    return this.Collection.AsQueryable()
                        .WhereExpressions(expressions)
                        .SkipIf(options?.Skip)
                        .TakeIf(options?.Take)
                        //.OrderByIf(options)
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

            return result.Select(d => this.Options.Mapper.Map<TEntity>(d));
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

            await this.Collection.InsertOneAsync(this.Options.Mapper.Map<TDestination>(entity)).AnyContext();
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                return entity;
            }

            var dEntity = this.Options.Mapper.Map<TDestination>(entity);
            await this.Collection.ReplaceOneAsync(e => e.Id.Equals(entity.Id), dEntity).AnyContext();
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

            this.logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogKeys.DomainRepository);
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

            var result = await this.Collection.DeleteOneAsync(e => e.Id.Equals(id)).AnyContext();
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

        protected Expression<Func<TDestination, bool>> EnsurePredicate(ISpecification<TEntity> specification)
        {
            return this.Options.Mapper.MapSpecification2<TEntity, TDestination>(specification);
        }

        protected virtual LambdaExpression EnsureExpression(LambdaExpression expression)
        {
            return this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(expression);
        }
    }
}
