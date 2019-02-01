namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Domain.Specifications;

    public class RepositorySpecificationDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly IRepository<TEntity> decoratee;
        private readonly ISpecification<TEntity> specification;

        public RepositorySpecificationDecorator(IRepository<TEntity> decoratee, ISpecification<TEntity> specification)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));
            EnsureArg.IsNotNull(specification, nameof(specification));

            this.decoratee = decoratee;
            this.specification = specification;
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            return await this.decoratee.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            return await this.decoratee.DeleteAsync(entity).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(new List<ISpecification<TEntity>>(), options, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.FindAllAsync(new List<ISpecification<TEntity>>(new[] { specification }), options, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.decoratee.FindAllAsync(
                new[] { this.specification }.Concat(specifications.Safe()),
                options, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return await this.decoratee.FindOneAsync(id).ConfigureAwait(false); // TODO: (tenant) this.specification?
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.decoratee.InsertAsync(entity).ConfigureAwait(false);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.decoratee.UpdateAsync(entity).ConfigureAwait(false);
        }

        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            return await this.decoratee.UpsertAsync(entity).ConfigureAwait(false);
        }
    }
}
