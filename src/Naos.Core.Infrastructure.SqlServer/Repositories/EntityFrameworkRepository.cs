namespace Naos.Core.Infrastructure.SqlServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using EnsureThat;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class EntityFrameworkRepository<TEntity> : IRepository<TEntity, Guid>
        where TEntity : TenantEntity<Guid>, IAggregateRoot
    {
        private readonly IMediator mediator;
        private readonly DbContext context;

        public EntityFrameworkRepository(IMediator mediator, DbContext context)
        {
            EnsureArg.IsNotNull(mediator);
            EnsureArg.IsNotNull(context);

            this.mediator = mediator;
            this.context = context;
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(int count = -1)
        {
            return await Task.FromResult(
                    this.context.Set<TEntity>().TakeIf(count).AsEnumerable());
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, int count = -1)
        {
            EnsureArg.IsNotNull(specification);

            return await Task.FromResult(
                this.context.Set<TEntity>()
                            .WhereExpression(specification.Expression())
                            .TakeIf(count));
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, int count = -1)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            EnsureArg.IsNotNull(specificationsArray);
            EnsureArg.IsTrue(specificationsArray.Length > 0);

            var expressions = specificationsArray.Select(s => s.Expression());
            return await Task.FromResult(
                this.context.Set<TEntity>()
                            .WhereExpressions(expressions)
                            .TakeIf(count));
        }

        public async Task<TEntity> FindByIdAsync(Guid id)
        {
            EnsureArg.IsTrue(id != Guid.Empty);

            return await this.context.Set<TEntity>().FirstOrDefaultAsync(o => o.Id == id).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            EnsureArg.IsTrue(id != Guid.Empty);

            return await this.FindByIdAsync(id) != null;
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entity)
        {
            EnsureArg.IsNotNull(entity);

            bool isNew = entity.Id == Guid.Empty;

            this.context.Set<TEntity>().Add(entity);
            await this.context.SaveChangesAsync().ConfigureAwait(false);

            if (isNew)
            {
                await this.mediator.Publish(new EntityAddedDomainEvent<TEntity, Guid>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<TEntity, Guid>(entity)).ConfigureAwait(false);
            }

            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            EnsureArg.IsTrue(id != Guid.Empty);

            var entity = await this.context.Set<TEntity>().FirstOrDefaultAsync(o => o.Id == id);
            this.context.Remove(entity);
            await this.context.SaveChangesAsync();
            await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity, Guid>(entity)).ConfigureAwait(false);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            EnsureArg.IsNotNull(entity);
            EnsureArg.IsNotNullOrEmpty(entity.Id.ToString());

            await this.DeleteAsync(entity.Id).ConfigureAwait(false);
        }
    }
}
