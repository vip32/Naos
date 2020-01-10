namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Domain;

    public class SqlServerDocumentRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly SqlServerDocumentRepositoryOptions<TEntity> options;

        public SqlServerDocumentRepository(SqlServerDocumentRepositoryOptions<TEntity> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));

            this.options = options;
            this.Logger = options.CreateLogger<SqlServerDocumentRepository<TEntity>>();

            this.Logger.LogInformation($"{{LogKey:l}} construct sql document repository (type={typeof(TEntity).PrettyName()})", LogKeys.DomainRepository);
        }

        public SqlServerDocumentRepository(Builder<SqlServerDocumentRepositoryOptionsBuilder<TEntity>, SqlServerDocumentRepositoryOptions<TEntity>> optionsBuilder)
            : this(optionsBuilder(new SqlServerDocumentRepositoryOptionsBuilder<TEntity>()).Build())
        {
        }

        public ILogger<SqlServerDocumentRepository<TEntity>> Logger { get; }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return ActionResult.None;
            }

            var result = await this.options.Provider.DeleteAsync(id).AnyContext();
            if(result == ProviderAction.Deleted)
            {
                return ActionResult.Deleted;
            }

            return ActionResult.None;
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            if (entity == null)
            {
                return ActionResult.None;
            }

            return await this.DeleteAsync(entity.Id).AnyContext();
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.options.Provider.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(Enumerable.Empty<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if (specification == null)
            {
                return await this.FindAllAsync(Enumerable.Empty<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
            }

            return await this.FindAllAsync(new[] { specification }, options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.options.Provider.LoadValuesAsync(
                expressions: specifications?.Select(s => s.ToExpression()),
                skip: options.Skip,
                take: options.Take,
                orderExpression: options.Order.Expression,
                orderDescending: options.Order.Direction == OrderDirection.Descending).ToEnumerable().AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            return (await this.options.Provider.LoadValuesAsync(id).ToEnumerable().AnyContext()).FirstOrDefault();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return (await this.UpsertAsync(entity).AnyContext()).entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return (await this.UpsertAsync(entity).AnyContext()).entity;
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
                this.options.IdGenerator.SetNew(entity);
            }

            if (this.options.PublishEvents && this.options.Mediator != null)
            {
                if (isNew)
                {
                    await this.options.Mediator.Publish(new EntityInsertDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.options.Mediator.Publish(new EntityUpdateDomainEvent(entity)).AnyContext();
                }
            }

            if (isNew)
            {
                if (entity is IStateEntity stateEntity)
                {
                    stateEntity.State.SetCreated();
                }
            }

            this.Logger.LogInformation($"{{LogKey:l}} upsert entity: {entity.GetType().PrettyName()}, isNew: {isNew}", LogKeys.DomainRepository);
            var result = await this.options.Provider.UpsertAsync(entity.Id, entity).AnyContext();

            if (this.options.PublishEvents && this.options.Mediator != null)
            {
                if (isNew)
                {
                    await this.options.Mediator.Publish(new EntityInsertedDomainEvent(entity)).AnyContext();
                }
                else
                {
                    await this.options.Mediator.Publish(new EntityUpdatedDomainEvent(entity)).AnyContext();
                }
            }

            return result switch
            {
                ProviderAction.Inserted => (entity, ActionResult.Inserted),
                ProviderAction.Updated => (entity, ActionResult.Updated),
                _ => (entity, ActionResult.None)
            };
        }
    }
}
