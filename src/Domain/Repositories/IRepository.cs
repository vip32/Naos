namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T>
        where T : class, IEntity, IAggregateRoot
    {
        Task<IEnumerable<T>> FindAllAsync(int count = -1);

        Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, int count = -1); // TODO: count should be part of specification

        Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, int count = -1); // TODO: count should be part of specification

        Task<T> FindOneAsync(object id);

        //Task<TEntity> FindOneAsync(ISpecification<TEntity> specification);

        Task<bool> ExistsAsync(object id);

        Task<T> AddOrUpdateAsync(T entity);

        Task DeleteAsync(object id);

        Task DeleteAsync(T entity);
    }
}