namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using LinqKit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Domain;

    public class EntityFrameworkRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public EntityFrameworkRepository(EntityFrameworkRepositoryOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.DbContext, nameof(options.DbContext));

            this.Options = options;
            this.Logger = options.CreateLogger<IGenericRepository<TEntity>>();

            try
            {
                var connectionString = this.Options.DbContext.Database.GetDbConnection().ConnectionString;
                this.Logger.LogInformation($"{{LogKey:l}} construct ef repository (type={typeof(TEntity).PrettyName()}, server={connectionString.SliceFrom("Server=").SliceTill(";")})", LogKeys.DomainRepository);

                if (connectionString.Equals("DataSource=:memory:", StringComparison.OrdinalIgnoreCase))
                {
                    // needed for sqlite inmemory
                    this.Options.DbContext.Database.OpenConnection();
                    this.Options.DbContext.Database.EnsureCreated();
                }
            }
            catch (InvalidOperationException)
            {
                // not possible for DbContext with UseInMemoryDatabase enabled (options)
                // 'Relational-specific methods can only be used when the context is using a relational database provider.'
            }
        }

        public EntityFrameworkRepository(Builder<EntityFrameworkRepositoryOptionsBuilder, EntityFrameworkRepositoryOptions> optionsBuilder)
            : this(optionsBuilder(new EntityFrameworkRepositoryOptionsBuilder()).Build())
        {
        }

        protected ILogger<IGenericRepository<TEntity>> Logger { get; }

        protected EntityFrameworkRepositoryOptions Options { get; }

        public async Task<IEnumerable<TEntity>> FindAllAsync(
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(Enumerable.Empty<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(
            ISpecification<TEntity> specification,
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(new[] { specification }, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(
            IEnumerable<ISpecification<TEntity>> specifications,
            IFindOptions<TEntity> options = null,
            CancellationToken cancellationToken = default)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            var expressions = specificationsArray.Safe().Select(s => s.ToExpression());

            if (options?.HasOrders() == true)
            {
                return await this.Options.DbContext.Set<TEntity>() // .AsAsyncEnumerable()
                    .AsExpandable()
                    .WhereExpressions(expressions)
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take)
                    .OrderByIf(options).ToListAsyncSafe(cancellationToken).AnyContext();
            }
            else
            {
                return await this.Options.DbContext.Set<TEntity>() // .AsAsyncEnumerable()
                    .AsExpandable()
                    .WhereExpressions(expressions)
                    .SkipIf(options?.Skip)
                    .TakeIf(options?.Take).ToListAsyncSafe(cancellationToken).AnyContext();
            }
        }

        public async Task<TEntity> FindOneAsync(object id) // partitionkey
        {
            if (id.IsDefault())
            {
                return null;
            }

            //#if NETSTANDARD2_0
            //            return await this.Options.DbContext.Set<TEntity>().FindAsync(this.TryParseGuid(id)).AnyContext();
            //#endif

            return await this.Options.DbContext.Set<TEntity>()
                .FindAsync(this.TidyId(id)).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.FindOneAsync(id).AnyContext() != null;
        }

        /// <summary>
        /// Inserts the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).AnyContext();
            return result.entity;
        }

        /// <summary>
        /// Updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            var result = await this.UpsertAsync(entity).AnyContext();
            return result.entity;
        }

        /// <summary>
        /// Insert or updates the provided entity.
        /// </summary>
        /// <param name="entity">The entity to insert or update.</param>
        public async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return (null, RepositoryActionResult.None);
            }

            var isNew = entity.Id.IsDefault();
            var existingEntity = isNew ? null : await this.FindOneAsync(entity.Id).AnyContext();
            isNew = isNew || existingEntity == null;

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

                this.Options.DbContext.Set<TEntity>().Add(entity);
            }
            else
            {
                this.Options.DbContext.Entry(existingEntity).CurrentValues.SetValues(entity);

                if (entity is IStateEntity stateEntity)
                {
                    stateEntity.State.SetUpdated();
                }
            }

            await this.Options.DbContext.SaveChangesAsync<TEntity>().AnyContext();

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
            return isNew ? (entity, RepositoryActionResult.Inserted) : (entity, RepositoryActionResult.Updated);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return RepositoryActionResult.None;
            }

            var existingEntity = await this.FindOneAsync(id).AnyContext();
            if (existingEntity != null)
            {
                this.Logger.LogInformation($"{{LogKey:l}} delete entity: {existingEntity.GetType().PrettyName()}, id: {existingEntity.Id}", LogKeys.DomainRepository);
                this.Options.DbContext.Remove(existingEntity);

                if (this.Options.PublishEvents && this.Options.Mediator != null)
                {
                    await this.Options.Mediator.Publish(new EntityDeleteDomainEvent(existingEntity)).AnyContext();
                }

                await this.Options.DbContext.SaveChangesAsync<TEntity>().AnyContext();

                if (this.Options.PublishEvents && this.Options.Mediator != null)
                {
                    await this.Options.Mediator.Publish(new EntityDeletedDomainEvent(existingEntity)).AnyContext();
                }

                return RepositoryActionResult.Deleted;
            }

            return RepositoryActionResult.None;
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            if (entity?.Id.IsDefault() != false)
            {
                return RepositoryActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).AnyContext();
        }

        private object TidyId(object value)
        {
            try
            {
                if (typeof(TEntity).GetProperty("Id")?.PropertyType == typeof(Guid) && value?.GetType() == typeof(string))
                {
                    // string to guid conversion
                    value = Guid.Parse(value.ToString());
                }
            }
            catch (FormatException ex)
            {
                throw new NaosClientFormatException(ex.Message, ex);
            }

            // TODO: more conversions needed? int?
            return value;
        }
    }
}
