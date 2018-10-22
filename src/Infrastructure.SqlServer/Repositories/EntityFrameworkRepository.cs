namespace Naos.Core.Infrastructure.SqlServer
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

    public class EntityFrameworkRepository<T> : IRepository<T>
        where T : class, IEntity, IAggregateRoot
    {
        private readonly IMediator mediator;
        private readonly DbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkRepository{T}" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="dbContext">The EF database context.</param>
        /// <param name="options">The options.</param>
        public EntityFrameworkRepository(IMediator mediator, DbContext dbContext, IRepositoryOptions options = null)
        {
            EnsureArg.IsNotNull(mediator);
            EnsureArg.IsNotNull(dbContext);

            this.mediator = mediator;
            this.dbContext = dbContext;
            this.Options = options;
        }

        protected IRepositoryOptions Options { get; }

        public async Task<IEnumerable<T>> FindAllAsync(IFindOptions<T> options = null)
        {
            return await Task.FromResult(
                    this.dbContext.Set<T>().TakeIf(options?.Take).AsEnumerable());
        }

        public async Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, IFindOptions<T> options = null)
        {
            return await Task.FromResult(
                this.dbContext.Set<T>()
                            .WhereExpression(specification?.ToExpression())
                            .SkipIf(options?.Skip)
                            .TakeIf(options?.Take));
        }

        public async Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, IFindOptions<T> options = null)
        {
            var specificationsArray = specifications as ISpecification<T>[] ?? specifications.ToArray();
            var expressions = specificationsArray.NullToEmpty().Select(s => s.ToExpression());

            return await Task.FromResult(
                this.dbContext.Set<T>()
                            .WhereExpressions(expressions)
                            .SkipIf(options?.Skip)
                            .TakeIf(options?.Take));
        }

        public async Task<T> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            return await this.dbContext.Set<T>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.FindOneAsync(id) != null;
        }

        public async Task<T> AddOrUpdateAsync(T entity)
        {
            if (entity == null)
            {
                return null;
            }

            bool isTransient = entity.Id.IsDefault(); // todo: add .IsTransient to Entity class

            this.dbContext.Set<T>().Add(entity);
            await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

            if (this.Options?.PublishEvents == true)
            {
                if (isTransient)
                {
                    await this.mediator.Publish(new EntityAddedDomainEvent<T>(entity)).ConfigureAwait(false);
                }
                else
                {
                    await this.mediator.Publish(new EntityUpdatedDomainEvent<T>(entity)).ConfigureAwait(false);
                }
            }

            return entity;
        }

        public async Task DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return;
            }

            var entity = await this.dbContext.Set<T>().FindAsync(id).ConfigureAwait(false);
            this.dbContext.Remove(entity);
            await this.dbContext.SaveChangesAsync();

            if (this.Options?.PublishEvents == true)
            {
                await this.mediator.Publish(new EntityDeletedDomainEvent<T>(entity)).ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(T entity)
        {
            if (entity == null || entity.Id.IsDefault())
            {
                return;
            }

            await this.DeleteAsync(entity.Id).ConfigureAwait(false);
        }
    }
}
