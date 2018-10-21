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

    public class EntityFrameworkRepository<T> : IRepository<T>
        where T : class, IEntity, IAggregateRoot
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

        public async Task<IEnumerable<T>> FindAllAsync(int? skip = null, int? take = null)
        {
            return await Task.FromResult(
                    this.context.Set<T>().TakeIf(take).AsEnumerable());
        }

        public async Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, int? skip = null, int? take = null)
        {
            return await Task.FromResult(
                this.context.Set<T>()
                            .WhereExpression(specification?.ToExpression())
                            .SkipIf(skip)
                            .TakeIf(take));
        }

        public async Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, int? skip = null, int? take = null)
        {
            var specificationsArray = specifications as ISpecification<T>[] ?? specifications.ToArray();
            var expressions = specificationsArray.NullToEmpty().Select(s => s.ToExpression());

            return await Task.FromResult(
                this.context.Set<T>()
                            .WhereExpressions(expressions)
                            .SkipIf(skip)
                            .TakeIf(take));
        }

        public async Task<T> FindOneAsync(object id)
        {
            if (id.IsDefault())
            {
                return null;
            }

            return await this.context.Set<T>().FindAsync(id).ConfigureAwait(false);
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

            bool isNew = entity.Id.IsDefault(); // todo: add .IsTransient to Entity class

            this.context.Set<T>().Add(entity);
            await this.context.SaveChangesAsync().ConfigureAwait(false);

            if (isNew)
            {
                await this.mediator.Publish(new EntityAddedDomainEvent<T>(entity)).ConfigureAwait(false);
            }
            else
            {
                await this.mediator.Publish(new EntityUpdatedDomainEvent<T>(entity)).ConfigureAwait(false);
            }

            return entity;
        }

        public async Task DeleteAsync(object id)
        {
            if (id.IsDefault())
            {
                return;
            }

            var entity = await this.context.Set<T>().FindAsync(id).ConfigureAwait(false);
            this.context.Remove(entity);
            await this.context.SaveChangesAsync();
            await this.mediator.Publish(new EntityDeletedDomainEvent<T>(entity)).ConfigureAwait(false);
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
