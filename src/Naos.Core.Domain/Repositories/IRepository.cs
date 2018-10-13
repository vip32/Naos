namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<TEntity, in TId>
        where TEntity : Entity<TId>, IAggregateRoot
    {
        Task<IEnumerable<TEntity>> FindAllAsync(int count = -1);

        Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, int count = -1); // TODO: count should be part of specification

        Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, int count = -1); // TODO: count should be part of specification

        Task<TEntity> FindByIdAsync(TId id);

        Task<bool> ExistsAsync(TId id);

        Task<TEntity> AddOrUpdateAsync(TEntity entity);

        Task DeleteAsync(TId id);

        Task DeleteAsync(TEntity entity);
    }
}