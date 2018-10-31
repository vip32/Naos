namespace Naos.Core.Infrastructure.EntityFramework
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using EnsureThat;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;
    using Naos.Core.Domain.Specifications;

    public class EntityFrameworkRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IMediator mediator;
        private readonly DbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkRepository{T}" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="dbContext">The EF database context.</param>
        /// <param name="options">The options.</param>
        public EntityFrameworkRepository(
            IMediator mediator,
            DbContext dbContext,
            IRepositoryOptions options = null)
        {
            EnsureArg.IsNotNull(mediator);
            EnsureArg.IsNotNull(dbContext);

            this.mediator = mediator;
            this.dbContext = dbContext;
            this.Options = options;
        }

        protected IRepositoryOptions Options { get; }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null)
        {
            return await Task.FromResult(
                    this.dbContext.Set<TEntity>().TakeIf(options?.Take).AsEnumerable());
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null)
        {
            return await Task.FromResult(
                this.dbContext.Set<TEntity>()
                            .WhereExpression(specification?.ToExpression())
                            .SkipIf(options?.Skip)
                            .TakeIf(options?.Take));
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            var expressions = specificationsArray.NullToEmpty().Select(s => s.ToExpression());

            return await Task.FromResult(
                this.dbContext.Set<TEntity>()
                            .WhereExpressions(expressions)
                            .SkipIf(options?.Skip)
                            .TakeIf(options?.Take));
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            return await this.dbContext.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
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
        public async Task<(TEntity entity, UpsertAction action)> UpsertAsync(TEntity entity)
        {
            if (entity == null)
            {
                return (null, UpsertAction.None);
            }

            bool isTransient = entity.Id.IsDefault(); // todo: add .IsTransient to Entity class
            this.dbContext.Set<TEntity>().Add(entity);

            if (this.Options?.PublishEvents != false)
            {
                if (isTransient)
                {
                    await this.mediator.Publish(new EntityInsertDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdateDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
            }

            await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

            if (this.Options?.PublishEvents != false)
            {
                if (isTransient)
                {
                    await this.mediator.Publish(new EntityInsertedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdatedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
                }
            }

#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
            return isTransient ? (entity, UpsertAction.Inserted) : (entity, UpsertAction.Updated);
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
        }

        public async Task DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return;
            }

            var entity = await this.dbContext.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
            this.dbContext.Remove(entity);

            if (this.Options?.PublishEvents != false)
            {
                await this.mediator.Publish(new EntityDeleteDomainEvent<TEntity>(entity)).ConfigureAwait(false);
            }

            await this.dbContext.SaveChangesAsync();

            if (this.Options?.PublishEvents != false)
            {
                await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(TEntity entity)
        {
            if (entity == null || entity.Id.IsDefault())
            {
                return;
            }

            await this.DeleteAsync(entity.Id).ConfigureAwait(false);
        }
    }
}
