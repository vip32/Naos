namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Specifications;

    public class RepositoryLoggingDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ILogger<IRepository<TEntity>> logger;
        private readonly IRepository<TEntity> decoratee;

        public RepositoryLoggingDecorator(ILogger<IRepository<TEntity>> logger, IRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.logger = logger;
            this.decoratee = decoratee;
        }

        public async Task DeleteAsync(object id)
        {
            await this.decoratee.DeleteAsync(id).ConfigureAwait(false);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await this.decoratee.DeleteAsync(entity).ConfigureAwait(false);
        }

        public async Task<bool> ExistsAsync(object id)
        {
            return await this.decoratee.ExistsAsync(id).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null)
        {
            return await this.decoratee.FindAllAsync(options).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null)
        {
            return await this.decoratee.FindAllAsync(specification, options).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            return await this.decoratee.FindAllAsync(specifications, options).ConfigureAwait(false);
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return await this.decoratee.FindOneAsync(id).ConfigureAwait(false);
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            return await this.decoratee.InsertAsync(entity).ConfigureAwait(false);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await this.decoratee.UpdateAsync(entity).ConfigureAwait(false);
        }

        public async Task<(TEntity entity, UpsertAction action)> UpsertAsync(TEntity entity)
        {
            return await this.decoratee.UpsertAsync(entity).ConfigureAwait(false);
        }
    }
}
