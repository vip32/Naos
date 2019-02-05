namespace Naos.Core.Infrastructure.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using EnsureThat;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class EntityFrameworkRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ILogger<IRepository<TEntity>> logger;
        private readonly IMediator mediator;
        private readonly DbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkRepository{T}" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="dbContext">The EF database context.</param>
        /// <param name="options">The options.</param>
        public EntityFrameworkRepository(
            ILogger<IRepository<TEntity>> logger,
            IMediator mediator,
            DbContext dbContext,
            IRepositoryOptions options = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(mediator, nameof(mediator));
            EnsureArg.IsNotNull(dbContext, nameof(dbContext));

            this.logger = logger;
            this.mediator = mediator;
            this.dbContext = dbContext;
            this.Options = options;

            try
            {
                if (dbContext.Database.GetDbConnection().ConnectionString.Equals("DataSource=:memory:", StringComparison.OrdinalIgnoreCase))
                {
                    // needed for sqlite inmemory
                    dbContext.Database.OpenConnection();
                    dbContext.Database.EnsureCreated();
                }
            }
            catch (InvalidOperationException)
            {
                // not possible for DbContext with UseInMemoryDatabase enabled (options)
                // 'Relational-specific methods can only be used when the context is using a relational database provider.'
            }
        }

        protected IRepositoryOptions Options { get; }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if (options?.HasOrders() == true)
            {
                return await this.dbContext.Set<TEntity>()
                    .TakeIf(options?.Take)
                    .OrderByIf(options).ToListAsyncSafe(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await this.dbContext.Set<TEntity>()
                    .TakeIf(options?.Take).ToListAsyncSafe(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if (options?.HasOrders() == true)
            {
                return await this.dbContext.Set<TEntity>()
                    .WhereExpression(specification?.ToExpression())
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take)
                    .OrderByIf(options).ToListAsyncSafe(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await this.dbContext.Set<TEntity>()
                    .WhereExpression(specification?.ToExpression())
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take).ToListAsyncSafe(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            var expressions = specificationsArray.Safe().Select(s => s.ToExpression());

            if (options?.HasOrders() == true)
            {
                return await this.dbContext.Set<TEntity>()
                    .WhereExpressions(expressions)
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take)
                    .OrderByIf(options).ToListAsyncSafe(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await this.dbContext.Set<TEntity>()
                    .WhereExpressions(expressions)
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take).ToListAsyncSafe(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            return await this.dbContext.Set<TEntity>().FindAsync(this.ConvertEntityId(id)).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
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
            var result = await this.UpsertAsync(entity).ConfigureAwait(false);
            return result.entity;
        }

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns></returns>
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).ConfigureAwait(false);
            return result.entity;
        }

        /// <summary>
        /// Insert or updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        /// <returns></returns>
        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return (null, ActionResult.None);
            }

            bool isNew = entity.Id.IsDefault() || !await this.ExistsAsync(entity.Id).ConfigureAwait(false);

            if (this.Options?.PublishEvents != false)
            {
                if (isNew)
                {
                    await this.mediator.Publish(new EntityInsertDomainEvent(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdateDomainEvent(entity)).ConfigureAwait(false);
                }
            }

            this.logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogEventKeys.DomainRepository);
            this.dbContext.Set<TEntity>().Add(entity);
            await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

            if (this.Options?.PublishEvents != false)
            {
                if (isNew)
                {
                    await this.mediator.Publish(new EntityInsertedDomainEvent(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdatedDomainEvent(entity)).ConfigureAwait(false);
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

            var entity = await this.FindOneAsync(id).ConfigureAwait(false);
            if (entity != null)
            {
                this.logger.LogInformation($"{{LogKey:l}} delete entity: {entity.GetType().PrettyName()}, id: {entity.Id}", LogEventKeys.DomainRepository);
                this.dbContext.Remove(entity);

                if (this.Options?.PublishEvents != false)
                {
                    await this.mediator.Publish(new EntityDeleteDomainEvent(entity)).ConfigureAwait(false);
                }

                await this.dbContext.SaveChangesAsync();

                if (this.Options?.PublishEvents != false)
                {
                    await this.mediator.Publish(new EntityDeletedDomainEvent(entity)).ConfigureAwait(false);
                }

                return ActionResult.Deleted;
            }

            return ActionResult.None;
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            if (entity == null || entity.Id.IsDefault())
            {
                return ActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).ConfigureAwait(false);
        }

        private object ConvertEntityId(object value)
        {
            try
            {
                if (typeof(TEntity).GetProperty("Id")?.PropertyType == typeof(Guid) && value?.GetType() == typeof(string))
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
