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
    using Naos.Foundation.Domain;

    public class CosmosSqlRepository<TEntity, TDestination> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot //, IDiscriminated
        where TDestination : class, ICosmosEntity
    {
        public CosmosSqlRepository(CosmosSqlRepositoryOptions<TEntity, TDestination> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));
            EnsureArg.IsNotNull(options.IdGenerator, nameof(options.IdGenerator));
            EnsureArg.IsNotNull(options.Mapper, nameof(options.Mapper));

            this.Options = options;
            this.Logger = options.CreateLogger<CosmosSqlRepository<TEntity, TDestination>>();
            this.Provider = options.Provider;

            this.Logger.LogInformation($"{{LogKey:l}} construct cosmos repository (type={typeof(TEntity).PrettyName()})", LogKeys.DomainRepository);
        }

        public CosmosSqlRepository(Builder<CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination>, CosmosSqlRepositoryOptions<TEntity, TDestination>> optionsBuilder)
            : this(optionsBuilder(new CosmosSqlRepositoryOptionsBuilder<TEntity, TDestination>()).Build())
        {
        }

        protected CosmosSqlRepositoryOptions<TEntity, TDestination> Options { get; }

        protected ILogger<CosmosSqlRepository<TEntity, TDestination>> Logger { get; }

        protected ICosmosSqlProvider<TDestination> Provider { get; }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(Enumerable.Empty<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(new[] { specification }, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            var expressions = specificationsArray.Safe()
                .Select(s => this.Options.Mapper.MapSpecification<TEntity, TDestination>(s).Expand()); // expand fixes Invoke in expression issue
            var order = (options?.Orders ?? new List<OrderOption<TEntity>>()).Insert(options?.Order).FirstOrDefault(); // cosmos only supports single orderby

            var result = await this.Provider
                .WhereAsync(
                    expressions: expressions,
                    skip: options?.Skip ?? -1,
                    take: options?.Take ?? -1,
                    orderExpression: this.Options.Mapper.MapExpression<Expression<Func<TDestination, object>>>(order?.Expression),
                    orderDescending: order?.Direction == OrderDirection.Descending).AnyContext();

            return result.Select(d => this.Options.Mapper.Map<TEntity>(d));
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return default;
            }

            return this.Options.Mapper.Map<TEntity>(await this.Provider.GetByIdAsync(id as string).AnyContext());
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
                return (default, RepositoryActionResult.None);
            }

            var isNew = entity.Id.IsDefault() || !await this.ExistsAsync(entity.Id).AnyContext();

            if (entity.Id.IsDefault())
            {
                this.Options.IdGenerator.SetNew(entity); // cosmos v3 needs an id, also for new documents
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

            if (isNew)
            {
                if (entity is IStateEntity stateEntity)
                {
                    stateEntity.State.SetCreated();
                }
            }
            else if (entity is IStateEntity stateEntity)
            {
                stateEntity.State.SetUpdated();
            }

            this.Logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogKeys.DomainRepository);
            var result = this.Options.Mapper.Map<TEntity>(
                await this.Provider.UpsertAsync(this.Options.Mapper.Map<TDestination>(entity)).AnyContext());

            if (this.Options.PublishEvents && this.Options.Mediator != null)
            {
                if (isNew)
                {
                    //await this.mediator.Publish(new EntityInsertedDomainEvent<IEntity>(result)).AnyContext();
                    await this.Options.Mediator.Publish(new EntityInsertedDomainEvent(result)).AnyContext();
                }
                else
                {
                    //await this.mediator.Publish(new EntityUpdatedDomainEvent<IEntity>(result)).AnyContext();
                    await this.Options.Mediator.Publish(new EntityUpdatedDomainEvent(result)).AnyContext();
                }
            }

            return isNew ? (result, RepositoryActionResult.Inserted) : (result, RepositoryActionResult.Updated);
        }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return RepositoryActionResult.None;
            }

            var entity = await this.FindOneAsync(id).AnyContext();
            if (entity != null)
            {
                return await this.DeleteAsync(entity).AnyContext();
            }

            return RepositoryActionResult.None;
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            if (entity?.Id.IsDefault() == true)
            {
                return RepositoryActionResult.None;
            }

            if (this.Options.PublishEvents && this.Options.Mediator != null)
            {
                await this.Options.Mediator.Publish(new EntityDeleteDomainEvent(entity)).AnyContext();
            }

            this.Logger.LogInformation($"{{LogKey:l}} delete entity: {entity.GetType().PrettyName()}, id: {entity.Id}", LogKeys.DomainRepository);
            var response = await this.Provider.DeleteByIdAsync(entity.Id as string).AnyContext();

            if (response)
            {
                if (this.Options.PublishEvents && this.Options.Mediator != null)
                {
                    await this.Options.Mediator.Publish(new EntityDeletedDomainEvent(entity)).AnyContext();
                }

                return RepositoryActionResult.Deleted;
            }

            return RepositoryActionResult.None;
        }
    }
}