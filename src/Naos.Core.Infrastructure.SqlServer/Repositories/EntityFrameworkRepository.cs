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

    public class EntityFrameworkRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
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
            return await Task.FromResult(
                this.context.Set<TEntity>()
                            .WhereExpression(specification?.Expression())
                            .TakeIf(count));
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, int count = -1)
        {
            var specificationsArray = specifications as ISpecification<TEntity>[] ?? specifications.ToArray();
            //EnsureArg.IsNotNull(specificationsArray);
            //EnsureArg.IsTrue(specificationsArray.Length > 0);

            var expressions = specificationsArray.NullToEmpty().Select(s => s.Expression());
            return await Task.FromResult(
                this.context.Set<TEntity>()
                            .WhereExpressions(expressions)
                            .TakeIf(count));
        }

        public async Task<TEntity> FindAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            return await this.context.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            if (id.IsDefault())
            {
                return false;
            }

            return await this.FindAsync(id) != null;
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            bool isNew = entity.Id.IsDefault(); // todo: add .IsTransient to Entity class

            this.context.Set<TEntity>().Add(entity);
            await this.context.SaveChangesAsync().ConfigureAwait(false);

            if (isNew)
            {
                await this.mediator.Publish(new EntityAddedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
            }

            return entity;
        }

        public async Task DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return;
            }

            var entity = await this.context.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
            this.context.Remove(entity);
            await this.context.SaveChangesAsync();
            await this.mediator.Publish(new EntityDeletedDomainEvent<TEntity>(entity)).ConfigureAwait(false);
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
