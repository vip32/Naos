namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Domain.Specifications;

    public abstract class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IRepository<TEntity> decoratee;

        protected Repository(IRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.decoratee = decoratee;
        }

        public virtual async Task<ActionResult> DeleteAsync(object id)
        {
            return await this.decoratee.DeleteAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            return await this.decoratee.DeleteAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(options, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(specification, options, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(specifications, options, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> FindOneAsync(object id)
        {
            return await this.decoratee.FindOneAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.decoratee.InsertAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.decoratee.UpdateAsync(entity).ConfigureAwait(false);
        }

        public virtual async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            return await this.decoratee.UpsertAsync(entity).ConfigureAwait(false);
        }
    }
}