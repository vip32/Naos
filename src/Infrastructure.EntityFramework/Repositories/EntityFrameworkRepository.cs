namespace Naos.Core.Infrastructure.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class EntityFrameworkRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ILogger<IRepository<TEntity>> logger;
        private readonly EntityFrameworkRepositoryOptions options;

        public EntityFrameworkRepository(EntityFrameworkRepositoryOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.DbContext, nameof(options.DbContext));

            this.options = options;
            this.logger = options.CreateLogger<IRepository<TEntity>>();

            try
            {
                if(this.options.DbContext.Database.GetDbConnection().ConnectionString.Equals("DataSource=:memory:", StringComparison.OrdinalIgnoreCase))
                {
                    // needed for sqlite inmemory
                    this.options.DbContext.Database.OpenConnection();
                    this.options.DbContext.Database.EnsureCreated();
                }
            }
            catch(InvalidOperationException)
            {
                // not possible for DbContext with UseInMemoryDatabase enabled (options)
                // 'Relational-specific methods can only be used when the context is using a relational database provider.'
            }
        }

        public EntityFrameworkRepository(Builder<EntityFrameworkRepositoryOptionsBuilder, EntityFrameworkRepositoryOptions> optionsBuilder)
            : this(optionsBuilder(new EntityFrameworkRepositoryOptionsBuilder()).Build())
        {
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if(options?.HasOrders() == true)
            {
                return await this.options.DbContext.Set<TEntity>()
                    .TakeIf(options?.Take)
                    .OrderByIf(options).ToListAsyncSafe(cancellationToken).AnyContext();
            }
            else
            {
                return await this.options.DbContext.Set<TEntity>()
                    .TakeIf(options?.Take).ToListAsyncSafe(cancellationToken).AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if(options?.HasOrders() == true)
            {
                return await this.options.DbContext.Set<TEntity>()
                    .WhereExpression(specification?.ToExpression())
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take)
                    .OrderByIf(options).ToListAsyncSafe(cancellationToken).AnyContext();
            }
            else
            {
                return await this.options.DbContext.Set<TEntity>()
                    .WhereExpression(specification?.ToExpression())
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take).ToListAsyncSafe(cancellationToken).AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            var expressions = specificationsArray.Safe().Select(s => s.ToExpression());

            if(options?.HasOrders() == true)
            {
                return await this.options.DbContext.Set<TEntity>()
                    .WhereExpressions(expressions)
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take)
                    .OrderByIf(options).ToListAsyncSafe(cancellationToken).AnyContext();
            }
            else
            {
                return await this.options.DbContext.Set<TEntity>()
                    .WhereExpressions(expressions)
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take).ToListAsyncSafe(cancellationToken).AnyContext();
            }
        }

        public async Task<TEntity> FindOneAsync(object id) // partitionkey
        {
            if(id.IsDefault())
            {
                return null;
            }

            return await this.options.DbContext.Set<TEntity>().FindAsync(this.TryParseGuid(id)).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if(id.IsDefault())
            {
                return false;
            }

            return await this.FindOneAsync(id) != null;
        }

        /// <summary>
        /// Inserts the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns></returns>
        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).AnyContext();
            return result.entity;
        }

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns></returns>
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).AnyContext();
            return result.entity;
        }

        /// <summary>
        /// Insert or updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        /// <returns></returns>
        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            if(entity == null)
            {
                return (null, ActionResult.None);
            }

            var isNew = entity.Id.IsDefault() || !await this.ExistsAsync(entity.Id).AnyContext();

            if(this.options.PublishEvents && this.options.Mediator != null)
            {
                if(isNew)
                {
                    await this.options.Mediator.Publish(new EntityInsertDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.options.Mediator.Publish(new EntityUpdateDomainEvent(entity)).AnyContext();
                }
            }

            this.logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogKeys.DomainRepository);
            if(isNew)
            {
                if(entity is IStateEntity stateEntity)
                {
                    stateEntity.State.SetCreated();
                }

                this.options.DbContext.Set<TEntity>().Add(entity);
            }
            else if(entity is IStateEntity stateEntity)
            {
                stateEntity.State.SetUpdated();
            }

            await this.options.DbContext.SaveChangesAsync<TEntity>().AnyContext();

            if(this.options.PublishEvents && this.options.Mediator != null)
            {
                if(isNew)
                {
                    await this.options.Mediator.Publish(new EntityInsertedDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.options.Mediator.Publish(new EntityUpdatedDomainEvent(entity)).AnyContext();
                }
            }

            //this.logger.LogInformation($"{{LogKey:l}} upserted entity: {entity.GetType().PrettyName()}, id: {entity.Id}, isNew: {isNew}", LogEventKeys.DomainRepository);
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            return isNew ? (entity, ActionResult.Inserted) : (entity, ActionResult.Updated);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            if(id.IsDefault())
            {
                return ActionResult.None;
            }

            var entity = await this.FindOneAsync(id).AnyContext();
            if(entity != null)
            {
                this.logger.LogInformation($"{{LogKey:l}} delete entity: {entity.GetType().PrettyName()}, id: {entity.Id}", LogKeys.DomainRepository);
                this.options.DbContext.Remove(entity);

                if(this.options.PublishEvents && this.options.Mediator != null)
                {
                    await this.options.Mediator.Publish(new EntityDeleteDomainEvent(entity)).AnyContext();
                }

                await this.options.DbContext.SaveChangesAsync<TEntity>().AnyContext();

                if(this.options.PublishEvents && this.options.Mediator != null)
                {
                    await this.options.Mediator.Publish(new EntityDeletedDomainEvent(entity)).AnyContext();
                }

                return ActionResult.Deleted;
            }

            return ActionResult.None;
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            if(entity?.Id.IsDefault() != false)
            {
                return ActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).AnyContext();
        }

        private object TryParseGuid(object value)
        {
            try
            {
                if(typeof(TEntity).GetProperty("Id")?.PropertyType == typeof(Guid) && value?.GetType() == typeof(string))
                {
                    // string to guid conversion
                    value = Guid.Parse(value.ToString());
                }
            }
            catch(FormatException ex)
            {
                throw new NaosClientFormatException(ex.Message, ex);
            }

            // TODO: more conversions needed? int?
            return value;
        }
    }
}
