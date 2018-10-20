namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        Task<IEnumerable<TEntity>> FindAllAsync(int count = -1);

        Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, int count = -1); // TODO: count should be part of specification

        Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, int count = -1); // TODO: count should be part of specification

        Task<TEntity> FindByIdAsync(object id);

        Task<bool> ExistsAsync(object id);

        Task<TEntity> AddOrUpdateAsync(TEntity entity);

        Task DeleteAsync(object id);

        Task DeleteAsync(TEntity entity);
    }
}