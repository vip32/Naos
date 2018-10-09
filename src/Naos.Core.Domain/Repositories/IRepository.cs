namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T, in TId>
        where T : Entity<TId>, IAggregateRoot
    {
        Task<IEnumerable<T>> FindAllAsync(int count = -1);

        Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, int count = -1); // TODO: count should be part of specification

        Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, int count = -1); // TODO: count should be part of specification

        Task<T> FindByIdAsync(TId id);

        Task<bool> ExistsAsync(TId id);

        Task<T> AddOrUpdateAsync(T entity);

        Task DeleteAsync(TId id);

        Task DeleteAsync(T entity);
    }
}