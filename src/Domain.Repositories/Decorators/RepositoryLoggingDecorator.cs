namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Naos.Core.Domain.Specifications;

    public class RepositoryLoggingDecorator<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public Task DeleteAsync(object id)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ExistsAsync(object id)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<TEntity> FindOneAsync(object id)
        {
            throw new System.NotImplementedException();
        }

        public Task<TEntity> InsertAsync(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public Task<(TEntity entity, UpsertAction action)> UpsertAsync(TEntity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}
