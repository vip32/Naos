namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    /// <summary>
    /// <para>Decorates an <see cref="Repositories.IGenericRepository{TEntity}"/>.</para>
    /// <para>
    ///    .-----------.
    ///    | Decorator |
    ///    `-----------`        .------------.
    ///          `------------> | decoratee  |
    ///            (forward)    `------------`
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IGenericRepository{TEntity}" />
    public class RepositorySoftDeleteDecorator<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot, IStateEntity
    {
        private readonly IGenericRepository<TEntity> decoratee;
        private readonly ISpecification<TEntity> specification;

        public RepositorySoftDeleteDecorator(IGenericRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.decoratee = decoratee;
            this.specification = new Specification<TEntity>(e => e.State.Deleted != true);
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
                entity.State.SetDeleted();
                var result = (await this.UpsertAsync(entity).AnyContext()).action;
                if(result == ActionResult.Updated)
                {
                    return ActionResult.Deleted;
                }
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

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(new List<ISpecification<TEntity>>(), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(new List<ISpecification<TEntity>>(new[] { specification }), options, cancellationToken).AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(
                new[] { this.specification }.Concat(specifications.Safe()),
                options, cancellationToken).AnyContext();
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            var entity = await this.decoratee.FindOneAsync(id).AnyContext();
            return this.specification.IsSatisfiedBy(entity) ? entity : null;
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.decoratee.InsertAsync(entity).AnyContext();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.decoratee.UpdateAsync(entity).AnyContext();
        }

        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            return await this.decoratee.UpsertAsync(entity).AnyContext();
        }
    }
}
